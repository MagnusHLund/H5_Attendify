let refreshPromise: Promise<boolean> | null = null;
let logoutInProgress = false;

function isAccessCodeSession(): boolean {
  return sessionStorage.getItem("auth-flow") === "access-code";
}

export async function fetchApi(
  endpoint: string,
  options?: RequestInit,
): Promise<Response> {
  let response = await fetch(endpoint, options);

  if (response.status !== 401 || logoutInProgress || isAccessCodeSession()) {
    return response;
  }

  const refreshed = await refreshAccessToken();
  if (refreshed) {
    response = await fetch(endpoint, options);
  }

  return response;
}

async function refreshAccessToken(): Promise<boolean> {
  if (logoutInProgress) {
    return false;
  }

  if (!refreshPromise) {
    refreshPromise = fetch("/api/auth/refresh", {
      method: "POST",
      credentials: "include",
    })
      .then((response) => response.ok)
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
}

export async function withRefreshPaused<T>(
  operation: () => Promise<T>,
): Promise<T> {
  logoutInProgress = true;

  try {
    await refreshPromise?.catch(() => false);
    return await operation();
  } finally {
    logoutInProgress = false;
  }
}
