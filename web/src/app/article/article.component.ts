import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { Article } from '../models/article';
import { MatChipInputEvent } from '@angular/material/chips';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { ApplicationService } from 'src/app/services/gadb.service';
import { TdLoadingService } from '@covalent/core/loading';
import { ArticleContext } from '../models/article-context';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { SelectLanguageComponent } from '../select-language/select-language.component';
import { Translation } from '../models/translation';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import * as BalloonEditor from '@ckeditor/ckeditor5-build-balloon';

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

  availableLanguages = ['ad', 'ae', 'af', 'ag', 'ai', 'al', 'am', 'ao', 'aq', 'ar', 'as', 'at', 'au', 'aw', 'ax', 'az', 'ba', 'bb', 'bd', 'be', 'bf', 'bg', 'bh', 'bi', 'bj', 'bl', 'bm', 'bn', 'bo', 'bq', 'br', 'bs', 'bt', 'bv', 'bw', 'by', 'bz', 'ca', 'cc', 'cd', 'cf', 'cg', 'ch', 'ci', 'ck', 'cl', 'cm', 'cn', 'co', 'cr', 'cu', 'cv', 'cw', 'cx', 'cy', 'cz', 'de', 'dj', 'dk', 'dm', 'do', 'dz', 'ec', 'ee', 'eg', 'eh', 'er', 'es', 'et', 'fi', 'fj', 'fk', 'fm', 'fo', 'fr', 'ga', 'gb', 'gd', 'ge', 'gf', 'gg', 'gh', 'gi', 'gl', 'gm', 'gn', 'gp', 'gq', 'gr', 'gs', 'gt', 'gu', 'gw', 'gy', 'hk', 'hm', 'hn', 'hr', 'ht', 'hu', 'id', 'ie', 'il', 'im', 'in', 'io', 'iq', 'ir', 'is', 'it', 'je', 'jm', 'jo', 'jp', 'ke', 'kg', 'kh', 'ki', 'km', 'kn', 'kp', 'kr', 'kw', 'ky', 'kz', 'la', 'lb', 'lc', 'li', 'lk', 'lr', 'ls', 'lt', 'lu', 'lv', 'ly', 'ma', 'mc', 'md', 'me', 'mf', 'mg', 'mh', 'mk', 'ml', 'mm', 'mn', 'mo', 'mp', 'mq', 'mr', 'ms', 'mt', 'mu', 'mv', 'mw', 'mx', 'my', 'mz', 'na', 'nc', 'ne', 'nf', 'ng', 'ni', 'nl', 'no', 'np', 'nr', 'nu', 'nz', 'om', 'pa', 'pe', 'pf', 'pg', 'ph', 'pk', 'pl', 'pm', 'pn', 'pr', 'ps', 'pt', 'pw', 'py', 'qa', 're', 'ro', 'rs', 'ru', 'rw', 'sa', 'sb', 'sc', 'sd', 'se', 'sg', 'sh', 'si', 'sj', 'sk', 'sl', 'sm', 'sn', 'so', 'sr', 'ss', 'st', 'sv', 'sx', 'sy', 'sz', 'tc', 'td', 'tf', 'tg', 'th', 'tj', 'tk', 'tl', 'tm', 'tn', 'to', 'tr', 'tt', 'tv', 'tw', 'tz', 'ua', 'ug', 'um', 'us', 'uy', 'uz', 'va', 'vc', 've', 'vg', 'vi', 'vn', 'vu', 'wf', 'ws', 'ye', 'yt', 'za', 'zm', 'zw'];

  visible = true;
  selectable = true;
  removable = true;
  addOnBlur = true;
  readonly separatorKeysCodes: number[] = [ENTER, COMMA];

  translationIndex: number = 0;
  articleText = '<Empty>';

  public Editor = BalloonEditor;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: ArticleContext, 
    private appService: ApplicationService,
    private loadingService: TdLoadingService,
    private dialog: MatDialog
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

    if (this.data.articleUpdated.hasText) {
      this.appService.getArticleText(this.data.articleUpdated.id)
      .subscribe(data => {
        this.data.text = data;
        this.data.textUpdated = data;
        this.loadingService.resolve("loading-text");

        this.copyArticleText(0);
      });
    }

    this.data.articleUpdated.translations.forEach(item => {
      if (item.hasText) {
        this.appService.getArticleText(this.data.articleUpdated.id, item.language)
        .subscribe(data => {
          this.data.translationText[item.language] = data;
          this.data.translationTextUpdated[item.language] = data;
        });  
      }
    });
  }

  onTabChanged(event: MatTabChangeEvent) {
    this.copyArticleText(event.index);
  }

  addTranslation() {
    const dialogRef = this.dialog.open(SelectLanguageComponent, {
      width: '300px',
      data: { languages: this.availableLanguages.filter(item => this.data.article.translations.map(l => l.language).indexOf(item) < 0 && item != this.data.article.language) }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.availableLanguages.indexOf(result) >= 0) {
        // Create translation...
        var translation = {
          title: null,
          subtitle: null,
          summary: null,
          date: null,
          language: result,
          status: null,
          translatedBy: null,
          reviewedBy: null,
          hasText: false,
          textLink: null,
          publications: null
        };

        var translationUpdated = {
          title: null,
          subtitle: null,
          summary: null,
          date: new Date(),
          language: result,
          status: null,
          translatedBy: null,
          reviewedBy: null,
          hasText: false,
          textLink: null,
          publications: null
        };

        this.appService.createTranslation(this.data.article.id, '', translationUpdated)
        .subscribe(() => {
          this.data.article.translations.push(translation);
          this.data.translationText[result] = '';
          this.data.articleUpdated.translations.push(translationUpdated);
          this.data.translationTextUpdated[result] = '';
        });
      }
    });
  }

  deleteTranslation() {
    if (this.translationIndex > 0) {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '300px',
        data: `This will delete the translation in '${this.data.article.translations[this.translationIndex - 1].language}' language and all its publications, are you sure?`
      });
  
      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.appService.deleteTranslation(this.data.article.id, this.data.article.translations[this.translationIndex - 1].language)
          .subscribe(() => {
            this.data.article.translations.splice(this.translationIndex - 1, 1);
            this.data.articleUpdated.translations.splice(this.translationIndex - 1, 1);
          });
        }
      });
    }
  }

  private copyArticleText(index: number) {
    if (index > 0) {
      let language = this.data.article.translations[index - 1].language;

      this.articleText = `<h1>${this.data.article.translations[index - 1].title ?? ''}</h1>
<h2>${this.data.article.translations[index - 1].subtitle ?? ''}</h2>
<p>${this.data.article.translations[index - 1].summary ?? ''}</p>
${this.data.translationTextUpdated[language] ?? ''}
`  
    } else {
      this.articleText = `<h1>${this.data.article.title ?? ''}</h1>
<h2>${this.data.article.subtitle ?? ''}</h2>
<p>${this.data.article.summary ?? ''}</p>
${this.data.textUpdated ?? ''}
`  
      }
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
