import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MainMenu } from './layout/main-menu/main-menu';

@Component({
  selector: 'app-root',
  imports: [MainMenu, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {}
