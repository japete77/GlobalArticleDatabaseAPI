import { Component, OnInit } from '@angular/core';
import { ApplicationService } from 'src/app/services/gadb.service';
import { Article } from '../models/article';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ArticleComponent } from '../article/article.component';
import { ArticleContext } from '../models/article-context';
import { AuthenticationService } from '../services/auth.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'articles',
  templateUrl: './articles.component.html',
  styleUrls: ['./articles.component.scss']
})
export class ArticlesComponent implements OnInit {

  constructor(
    private appService: ApplicationService,
    private _iconRegistry: MatIconRegistry, 
    private _domSanitizer:DomSanitizer,
    public dialog: MatDialog,
    private authService: AuthenticationService,
    private route: ActivatedRoute) {
    this._iconRegistry.addSvgIconInNamespace('assets', 'gadb',
    this._domSanitizer.bypassSecurityTrustResourceUrl('assets/gadb.svg'));
  }
  
  PAGE_SIZE = 25
  currentPage = 1
  showMore = false
  loading = true
  total = 0
  articles: Article[] = []
 
  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      let articleId = params.get('article')
      if (articleId) {
        this.appService.getArticle(articleId)
        .subscribe(data => {
          this.openArticle(data)
        });
      }
    });

    this.appService.getSearchResponse().subscribe(data => {
      if (data) {
        this.total = data.total
        this.currentPage = data.currentPage
        if (data.currentPage == 1) this.articles = []
        this.articles = this.articles.concat(data.articles)
        this.loading = false
        this.showMore = (this.currentPage * this.PAGE_SIZE) < data.total
      }
    });

    this.appService.getSearchRequest().subscribe(filter => {
      if (filter && filter.page < 2) this.articles = []
      this.showMore = false
      this.loading = true      
    });

    this.authService.getLogoutRequest().subscribe(() => {
      this.articles = []
      this.showMore = false
      this.loading = true
    })

    this.loading = true
  }

  loadMore() {
    this.appService.loadMore()
  }

  openNewTab(link: string) {
    window.open(link, "_blank");
  }

  openArticle(article: Article) {
    const dialogConfig = new MatDialogConfig()
    dialogConfig.data = {
      article: article
    }

    const dialogRef = this.dialog.open(ArticleComponent, dialogConfig)

    dialogRef.updateSize("100vw")

    dialogRef.afterClosed().subscribe((data: ArticleContext) => {
      
      var allPromises = []

      if (data) {
        const updateArticle = JSON.stringify(data.article) != JSON.stringify(data.articleUpdated)
        
        if (updateArticle) {
          allPromises.push(this.appService.updateArticle(data.articleUpdated).toPromise())
        }

        if (data.text != data.textUpdated) {
          allPromises.push(this.appService.updateArticleText(data.articleUpdated.id, data.textUpdated).toPromise())
        }
        
        data.article.translations.forEach(item => {

          if (data.translationTextUpdated[item.language]) {
            item.hasText = true
          }

          if (JSON.stringify(item) != JSON.stringify(data.articleUpdated.translations.find(s => s.language == item.language))) {
            allPromises.push(this.appService.updateTranslation(data.articleUpdated.id, data.articleUpdated.translations.find(s => s.language == item.language)).toPromise())
          }

          if (data.translationText[item.language] != data.translationTextUpdated[item.language]) {
            allPromises.push(this.appService.updateTranslationText(data.articleUpdated.id, data.translationTextUpdated[item.language], item.language).toPromise())
          }

        })

        if (allPromises.length > 0) {
          Promise.all(allPromises).then(() => {
            // Retrieve article...
            this.appService.getArticle(article.id).subscribe(articleUpdated => {
              article.author = articleUpdated.author
              article.category = articleUpdated.category
              article.date = articleUpdated.date
              article.hasText = articleUpdated.hasText
              article.imageLink = articleUpdated.imageLink
              article.language = articleUpdated.language
              article.owner = articleUpdated.owner
              article.sourceLink = articleUpdated.sourceLink
              article.subtitle = articleUpdated.subtitle
              article.summary = articleUpdated.summary
              article.textLink = articleUpdated.textLink
              article.title = articleUpdated.title
              article.topics = articleUpdated.topics
              article.translations = articleUpdated.translations
            })
          })
        }    
      }
    })
  }
}
