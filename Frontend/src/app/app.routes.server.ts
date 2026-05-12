import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: '**',
    // Avoid build-time prerendering for parameterized routes (e.g. /orders/:id)
    // so the Docker production build can complete without getPrerenderParams.
    renderMode: RenderMode.Server
  }
];
