import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Article } from '../models/article';
import { MatChipInputEvent } from '@angular/material/chips';
import {COMMA, ENTER} from '@angular/cdk/keycodes';
import { ApplicationService } from 'src/services/gadb.service';
import { TdLoadingService } from '@covalent/core/loading';

@Component({
  selector: 'app-article',
  templateUrl: './article.component.html',
  styleUrls: ['./article.component.scss']
})
export class ArticleComponent implements OnInit {

  article: Article;
  text: string;
  translatedTexts = [];

  visible = true;
  selectable = true;
  removable = true;
  addOnBlur = true;
  readonly separatorKeysCodes: number[] = [ENTER, COMMA];

  constructor(
    @Inject(MAT_DIALOG_DATA) data, 
    private appService: ApplicationService,
    private loadingService: TdLoadingService
  ) { 
    this.article = data;
  }

  ngOnInit(): void {
    this.loadingService.register("loading-text")

    this.appService.getArticleText(this.article.id)
      .subscribe(data => {
        this.text = data;
        this.loadingService.resolve("loading-text");
      });

    this.article.translations.forEach(item => {
      this.appService.getArticleText(this.article.id, item.language)
      .subscribe(data => {
        this.translatedTexts[item.language] = data;
      });
    });
  }

  add(event: MatChipInputEvent): void {
    const input = event.input;
    const value = event.value;

    // Add our fruit
    if ((value || '').trim()) {
      this.article.topics.push(value.trim());
    }

    // Reset the input value
    if (input) {
      input.value = '';
    }
  }

  remove(topic: string): void {
    const index = this.article.topics.indexOf(topic);

    if (index >= 0) {
      this.article.topics.splice(index, 1);
    }
  }
}
