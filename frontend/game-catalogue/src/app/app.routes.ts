import { Routes } from '@angular/router';
import { BrowseComponent } from './pages/browse/browse';
import { EditComponent } from './pages/edit/edit';

export const routes: Routes = [
  { path: '', redirectTo: '/browse', pathMatch: 'full' },
  { path: 'browse', component: BrowseComponent },
  { path: 'games/new/edit', component: EditComponent },
  { path: 'games/:id/edit', component: EditComponent }
];
