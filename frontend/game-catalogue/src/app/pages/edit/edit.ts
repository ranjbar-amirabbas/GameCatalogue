import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { GameService } from '../../services/game.service';
import { GENRES, Genre, PLATFORMS, Platform, SaveGameCommand } from '../../models/game.model';

@Component({
  selector: 'app-edit',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit.html',
  styleUrl: './edit.scss'
})
export class EditComponent implements OnInit {
  readonly genres = GENRES;
  readonly platforms = PLATFORMS;

  id: string | null = null;
  saving = false;
  error: string | null = null;

  model: SaveGameCommand = {
    title: '',
    genre: 'Action' as Genre,
    platform: 'PC' as Platform,
    releaseDate: new Date().toISOString().substring(0, 10),
    developer: '',
    rating: 0,
    downloadCount: 0
  };

  coverPreview: string | null = null;
  selectedFile: File | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly gameService: GameService
  ) {}

  get isEdit(): boolean {
    return this.id !== null;
  }

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    if (this.id) {
      this.gameService.getGameById(this.id).subscribe({
        next: (game) => {
          this.model = {
            title: game.title,
            genre: game.genre,
            platform: game.platform,
            releaseDate: game.releaseDate.substring(0, 10),
            developer: game.developer,
            rating: game.rating,
            downloadCount: game.downloadCount
          };
          this.coverPreview = game.coverImageUrl || null;
        },
        error: () => (this.error = 'Failed to load game.')
      });
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      const reader = new FileReader();
      reader.onload = () => (this.coverPreview = reader.result as string);
      reader.readAsDataURL(this.selectedFile);
    }
  }

  save(): void {
    this.saving = true;
    this.error = null;

    if (this.isEdit && this.id) {
      // Update existing game data, then upload a new cover image if one was chosen.
      this.gameService.updateGame(this.id, this.model).subscribe({
        next: () => this.uploadThenLeave(this.id!),
        error: () => this.handleError()
      });
    } else {
      // Create with the image in a single multipart request.
      this.gameService.createGame(this.model, this.selectedFile).subscribe({
        next: () => this.leave(),
        error: () => this.handleError()
      });
    }
  }

  private uploadThenLeave(id: string): void {
    if (this.selectedFile) {
      this.gameService.uploadCoverImage(id, this.selectedFile).subscribe({
        next: () => this.leave(),
        error: () => this.handleError()
      });
    } else {
      this.leave();
    }
  }

  private handleError(): void {
    this.saving = false;
    this.error = 'Failed to save game. Please check the form and try again.';
  }

  private leave(): void {
    this.saving = false;
    this.router.navigate(['/browse']);
  }

  cancel(): void {
    this.router.navigate(['/browse']);
  }
}
