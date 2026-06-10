export const environment = {
  production: true,
  // In the container the app is served by nginx, which reverse-proxies
  // /api to the API service. Using a relative URL keeps it same-origin.
  apiUrl: ''
};
