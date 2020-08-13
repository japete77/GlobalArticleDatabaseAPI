import { Component, OnInit, OnDestroy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatChipInputEvent } from '@angular/material/chips';
import { ApplicationService } from 'src/app/services/gadb.service';
import { ArticleContext } from '../models/article-context';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { SelectLanguageComponent } from '../select-language/select-language.component';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { SelectItemComponent } from '../select-item/select-item.component';
import * as BallonEditor from '@ckeditor/ckeditor5-build-balloon';
import { ActivatedRoute } from '@angular/router';
import { first } from 'rxjs/operators';

@Component({
  selector: 'app-article',
  templateUrl: './article.component.html',
  styleUrls: ['./article.component.scss']
})
export class ArticleComponent implements OnInit, OnDestroy {

  availableLanguages = ['ad', 'ae', 'af', 'ag', 'ai', 'al', 'am', 'ao', 'aq', 'ar', 'as', 'at', 'au', 'aw', 'ax', 'az', 'ba', 'bb', 'bd', 'be', 'bf', 'bg', 'bh', 'bi', 'bj', 'bl', 'bm', 'bn', 'bo', 'bq', 'br', 'bs', 'bt', 'bv', 'bw', 'by', 'bz', 'ca', 'cc', 'cd', 'cf', 'cg', 'ch', 'ci', 'ck', 'cl', 'cm', 'cn', 'co', 'cr', 'cu', 'cv', 'cw', 'cx', 'cy', 'cz', 'de', 'dj', 'dk', 'dm', 'do', 'dz', 'ec', 'ee', 'eg', 'eh', 'er', 'es', 'et', 'fi', 'fj', 'fk', 'fm', 'fo', 'fr', 'ga', 'gb', 'gd', 'ge', 'gf', 'gg', 'gh', 'gi', 'gl', 'gm', 'gn', 'gp', 'gq', 'gr', 'gs', 'gt', 'gu', 'gw', 'gy', 'hk', 'hm', 'hn', 'hr', 'ht', 'hu', 'id', 'ie', 'il', 'im', 'in', 'io', 'iq', 'ir', 'is', 'it', 'je', 'jm', 'jo', 'jp', 'ke', 'kg', 'kh', 'ki', 'km', 'kn', 'kp', 'kr', 'kw', 'ky', 'kz', 'la', 'lb', 'lc', 'li', 'lk', 'lr', 'ls', 'lt', 'lu', 'lv', 'ly', 'ma', 'mc', 'md', 'me', 'mf', 'mg', 'mh', 'mk', 'ml', 'mm', 'mn', 'mo', 'mp', 'mq', 'mr', 'ms', 'mt', 'mu', 'mv', 'mw', 'mx', 'my', 'mz', 'na', 'nc', 'ne', 'nf', 'ng', 'ni', 'nl', 'no', 'np', 'nr', 'nu', 'nz', 'om', 'pa', 'pe', 'pf', 'pg', 'ph', 'pk', 'pl', 'pm', 'pn', 'pr', 'ps', 'pt', 'pw', 'py', 'qa', 're', 'ro', 'rs', 'ru', 'rw', 'sa', 'sb', 'sc', 'sd', 'se', 'sg', 'sh', 'si', 'sj', 'sk', 'sl', 'sm', 'sn', 'so', 'sr', 'ss', 'st', 'sv', 'sx', 'sy', 'sz', 'tc', 'td', 'tf', 'tg', 'th', 'tj', 'tk', 'tl', 'tm', 'tn', 'to', 'tr', 'tt', 'tv', 'tw', 'tz', 'ua', 'ug', 'um', 'us', 'uy', 'uz', 'va', 'vc', 've', 'vg', 'vi', 'vn', 'vu', 'wf', 'ws', 'ye', 'yt', 'za', 'zm', 'zw']

  translationIndex: number = 0
  articleText = '<Empty>'
  id: string
  subscription: any
  data: ArticleContext
  loading = true

  public Editor = BallonEditor;

  constructor(
    private appService: ApplicationService,
    private route: ActivatedRoute,
    private dialog: MatDialog
  ) {

    this.subscription = this.route.params.subscribe(params => {
      this.id = params['id']

      this.appService.getInitialized().subscribe(init => {
        if (init) {
          this.appService.getArticle(this.id)
            .pipe(first())
            .subscribe(val => {

              this.data = {
                article: val,
                authors: this.appService.getAuthors(),
                categories: this.appService.getCategories(),
                owners: this.appService.getOwners(),
                topics: this.appService.getTopics(),
                status: this.appService.getTranslationStatuses(),
                text: null,
                textUpdated: null,
                articleUpdated: JSON.parse(JSON.stringify(val)),
                translationText: [],
                translationTextUpdated: [],
              }

              this.data.articleUpdated.translations.forEach(item => {
                this.data.translationText[item.language] = ''
                this.data.translationTextUpdated[item.language] = ''
              })

              var promises = [];

              promises.push(this.appService.getArticleText(this.data.articleUpdated.id).toPromise())
              if (this.data.articleUpdated.hasText) {
                promises.push(this.appService.getArticleText(this.data.articleUpdated.id).toPromise())
              }

              this.data.articleUpdated.translations.forEach(item => {
                if (item.hasText) {
                  promises.push(this.appService.getArticleText(this.data.articleUpdated.id, item.language).toPromise())
                }
              });

              if (promises.length == 0) {
                this.loading = false
              } else {
                Promise.all(promises).then(results => {
                  var index = 1;
                  if (this.data.articleUpdated.hasText) {
                    this.data.text = results[index];
                    this.data.textUpdated = results[index];
                    index++;
                    this.copyArticleText(0);
                  }

                  for (var r = 0; r < this.data.articleUpdated.translations.length; r++) {
                    if (this.data.articleUpdated.translations[r].hasText) {
                      this.data.translationText[this.data.articleUpdated.translations[r].language] = results[index];
                      this.data.translationTextUpdated[this.data.articleUpdated.translations[r].language] = results[index];
                      index++;
                    }
                  }

                  this.loading = false
                })
              }
            })
        }
      });
    })
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe()
  }

  ngOnInit(): void {
    this.loading = true
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

    // Add topic
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

  editAuthors() {
    const dialogRef = this.dialog.open(SelectItemComponent, {
      width: '600px',
      data: {
        title: "Please select the authors of the article from the list:",
        selected: this.data.articleUpdated.author,
        list: this.data.authors
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.data.articleUpdated.author = result;
      }
    });
  }

  editTopics() {
    const dialogRef = this.dialog.open(SelectItemComponent, {
      width: '600px',
      data: {
        title: "Please select the topics of the article from the list:",
        selected: this.data.articleUpdated.topics,
        list: this.data.topics
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.data.articleUpdated.topics = result;
      }
    });
  }

  openSourceLink() {
    window.open(this.data.articleUpdated.sourceLink, "_blank");
  }

  save() {
    this.loading = true

    var allPromises = []

    if (this.data) {
      const updateArticle = JSON.stringify(this.data.article) != JSON.stringify(this.data.articleUpdated)

      if (updateArticle) {
        allPromises.push(this.appService.updateArticle(this.data.articleUpdated).toPromise())
      }

      if (this.data.text != this.data.textUpdated) {
        allPromises.push(this.appService.updateArticleText(this.data.articleUpdated.id, this.data.textUpdated).toPromise())
      }

      this.data.article.translations.forEach(item => {

        if (this.data.translationTextUpdated[item.language]) {
          item.hasText = true
        }

        if (JSON.stringify(item) != JSON.stringify(this.data.articleUpdated.translations.find(s => s.language == item.language))) {
          allPromises.push(this.appService.updateTranslation(this.data.articleUpdated.id, this.data.articleUpdated.translations.find(s => s.language == item.language)).toPromise())
        }

        if (this.data.translationText[item.language] != this.data.translationTextUpdated[item.language]) {
          allPromises.push(this.appService.updateTranslationText(this.data.articleUpdated.id, this.data.translationTextUpdated[item.language], item.language).toPromise())
        }
      })

      if (allPromises.length > 0) {
        Promise.all(allPromises).then(() => {
          this.loading = false
        })
      } else {
        this.loading = false
      }
    }
  }
}
