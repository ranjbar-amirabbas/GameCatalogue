import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { GameService } from '../../services/game.service';
import { Game, GENRES, Genre, PLATFORMS, Platform } from '../../models/game.model';

@Component({
  selector: 'app-browse',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbPaginationModule],
  templateUrl: './browse.html',
  styleUrl: './browse.scss'
})
export class BrowseComponent implements OnInit {
  readonly genres = GENRES;
  readonly platforms = PLATFORMS;

  games: Game[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 10;
  loading = false;
  error: string | null = null;

  selectedGenre: Genre | null = null;
  selectedPlatform: Platform | null = null;
  searchTerm = '';

  constructor(private readonly gameService: GameService, private readonly router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;
    this.gameService
      .getGames(this.page, this.pageSize, this.selectedGenre, this.selectedPlatform, this.searchTerm)
      .subscribe({
        next: (result) => {
          this.games = result.items;
          this.totalCount = result.totalCount;
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to load games. Is the API running?';
          this.loading = false;
        }
      });
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  onPageChange(page: number): void {
    this.page = page;
    this.load();
  }

  add(): void {
    this.router.navigate(['/games/new/edit']);
  }

  edit(game: Game): void {
    this.router.navigate(['/games', game.id, 'edit']);
  }

  remove(game: Game): void {
    if (!confirm(`Delete "${game.title}"?`)) {
      return;
    }
    this.gameService.deleteGame(game.id).subscribe({
      next: () => this.load(),
      error: () => (this.error = 'Failed to delete game.')
    });
  }
}
