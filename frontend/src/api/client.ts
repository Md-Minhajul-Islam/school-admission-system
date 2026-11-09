import { API_BASE_URL } from "../config";
import type {
  ClassCreated,
  ClassSummary,
  ConfirmedHold,
  HoldCreated,
  HoldDetail,
  MyHold,
} from "./types";

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

const STATUS_MESSAGES: Record<number, string> = {
  400: "Please check the information you entered and try again.",
  403: "You can only change applications that belong to you.",
  404: "We could not find that record. It may have been removed.",
  409: "That action is not possible right now. Please refresh and try again.",
  500: "The admission server ran into a problem. Please try again.",
  503: "The admission server is temporarily unavailable. Please try again in a moment.",
};

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export function errorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return error.message;
  }

  if (error instanceof Error && error.message.trim()) {
    return error.message;
  }

  return "Something went wrong. Please try again.";
}

async function readProblem(response: Response): Promise<ProblemDetails | null> {
  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("json")) {
    return null;
  }

  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return null;
  }
}

async function describeFailure(response: Response): Promise<string> {
  const problem = await readProblem(response);

  if (problem?.errors) {
    const firstMessage = Object.values(problem.errors)
      .flat()
      .find((message) => typeof message === "string" && message.trim().length > 0);

    if (firstMessage) {
      return firstMessage;
    }
  }

  if (problem?.detail?.trim()) {
    return problem.detail;
  }

  return (
    STATUS_MESSAGES[response.status] ??
    `The request failed (HTTP ${response.status}). Please try again.`
  );
}

interface RequestOptions {
  method?: "GET" | "POST" | "DELETE";
  body?: unknown;
  studentId?: string;
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: "application/json" };

  if (options.body !== undefined) {
    headers["Content-Type"] = "application/json";
  }

  if (options.studentId) {
    headers["X-Student-Id"] = options.studentId;
  }

  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      method: options.method ?? "GET",
      headers,
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
    });
  } catch {
    throw new ApiError(
      0,
      "We could not reach the admission server. Please check your connection and try again.",
    );
  }

  if (!response.ok) {
    throw new ApiError(response.status, await describeFailure(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

function pathSegment(value: string): string {
  return encodeURIComponent(value);
}

export const api = {
  createClass(input: { name: string; totalSeats: number }): Promise<ClassCreated> {
    return request<ClassCreated>("/classes", { method: "POST", body: input });
  },

  listClasses(): Promise<ClassSummary[]> {
    return request<ClassSummary[]>("/classes");
  },

  getClass(classId: string): Promise<ClassSummary> {
    return request<ClassSummary>(`/classes/${pathSegment(classId)}`);
  },

  applyForClass(classId: string, studentId: string): Promise<HoldCreated> {
    return request<HoldCreated>(`/classes/${pathSegment(classId)}/holds`, {
      method: "POST",
      studentId,
    });
  },

  getHold(holdId: string): Promise<HoldDetail> {
    return request<HoldDetail>(`/holds/${pathSegment(holdId)}`);
  },

  confirmHold(holdId: string, studentId: string): Promise<ConfirmedHold> {
    return request<ConfirmedHold>(`/holds/${pathSegment(holdId)}/confirm`, {
      method: "POST",
      studentId,
    });
  },

  cancelHold(holdId: string, studentId: string): Promise<void> {
    return request<void>(`/holds/${pathSegment(holdId)}`, {
      method: "DELETE",
      studentId,
    });
  },

  getMyHolds(studentId: string): Promise<MyHold[]> {
    return request<MyHold[]>("/me/holds", { studentId });
  },
};
