import { Component, type ErrorInfo, type ReactNode } from "react";

interface Props {
  children: ReactNode;
}

interface State {
  error: Error | null;
}

/**
 * Prevents a rendering failure from turning into a blank page. The fallback is plain
 * language and offers a recovery action.
 */
export class ErrorBoundary extends Component<Props, State> {
  public state: State = { error: null };

  public static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  public componentDidCatch(error: Error, info: ErrorInfo): void {
    console.error("Unexpected UI failure", error, info);
  }

  public render(): ReactNode {
    if (this.state.error) {
      return (
        <div className="mx-auto mt-20 max-w-lg rounded-2xl border border-rose-200 bg-white p-8 text-center shadow-sm">
          <h1 className="text-xl font-bold text-slate-900">
            The page could not be displayed
          </h1>
          <p className="mt-3 text-sm text-slate-600">
            Something unexpected happened in the browser. Your applications are safe on
            the server. Reload the page to continue.
          </p>
          <button
            type="button"
            onClick={() => window.location.reload()}
            className="mt-6 rounded-xl bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700"
          >
            Reload page
          </button>
        </div>
      );
    }

    return this.props.children;
  }
}
