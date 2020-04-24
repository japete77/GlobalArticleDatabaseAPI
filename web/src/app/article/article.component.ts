import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Article } from '../models/article';
import { MatChipInputEvent } from '@angular/material/chips';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { ApplicationService } from 'src/app/services/gadb.service';
import { TdLoadingService } from '@covalent/core/loading';
import { ArticleContext } from '../models/article-context';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';

@Component({
  selector: 'app-article',
  templateUrl: './article.component.html',
  styleUrls: ['./article.component.scss']
})
export class ArticleComponent implements OnInit {

  authors: string[];
  categories: string[];
  owners: string[];
  topics: string[];
  status: string[];

  visible = true;
  selectable = true;
  removable = true;
  addOnBlur = true;
  readonly separatorKeysCodes: number[] = [ENTER, COMMA];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: ArticleContext, 
    private appService: ApplicationService,
    private loadingService: TdLoadingService
  ) { 
    data.articleUpdated = JSON.parse(JSON.stringify(data.article));
    data.translationText = [];
    data.translationTextUpdated = [];
    data.articleUpdated.translations.forEach(item => {
      data.translationText[item.language] = '';
      data.translationTextUpdated[item.language] = '';
    });
    this.authors = data.authors;
    this.categories = data.categories;
    this.owners = data.owners;
    this.topics = data.topics;
    this.status = data.status;
  }

  ngOnInit(): void {
    this.loadingService.register("loading-text")

    this.appService.getArticleText(this.data.articleUpdated.id)
      .subscribe(data => {
        this.data.text = data;
        this.data.textUpdated = data;
        this.loadingService.resolve("loading-text");
      });

    this.data.articleUpdated.translations.forEach(item => {
      this.appService.getArticleText(this.data.articleUpdated.id, item.language)
      .subscribe(data => {
        this.data.translationText[item.language] = data;
        this.data.translationTextUpdated[item.language] = data;
      });
    });
  }

  articleCreationDateChange(event) {
  }

  add(event: MatChipInputEvent): void {
    const input = event.input;
    const value = event.value;

    // Add our fruit
    if ((value || '').trim()) {
      this.data.articleUpdated.topics.push(value.trim());
    }

    // Reset the input value
    if (input) {
      input.value = '';
    }
  }

  remove(topic: string): void {
    const index = this.data.articleUpdated.topics.indexOf(topic);

    if (index >= 0) {
      this.data.articleUpdated.topics.splice(index, 1);
    }
  }
}
