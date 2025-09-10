export const environment = {
  production: false,
  // API is internal behind Traefik; use hostname routed by Traefik
    hubUrl: 'http://localhost:8082/hubs/schedulerHub'
};
