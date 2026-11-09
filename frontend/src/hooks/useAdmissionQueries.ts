import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";

export function useClassesQuery() {
  return useQuery({
    queryKey: ["classes"],
    queryFn: api.listClasses,
    refetchInterval: 3000,
    refetchIntervalInBackground: true,
  });
}

export function useMyHoldsQuery(studentId: string, enabled: boolean) {
  return useQuery({
    queryKey: ["my-holds", studentId],
    queryFn: () => api.getMyHolds(studentId),
    enabled,
    refetchInterval: 2000,
    refetchIntervalInBackground: true,
  });
}

export function useHoldQuery(holdId: string | null) {
  return useQuery({
    queryKey: ["hold", holdId],
    queryFn: () => api.getHold(holdId as string),
    enabled: Boolean(holdId),
    refetchInterval: 1000,
    refetchIntervalInBackground: true,
  });
}
