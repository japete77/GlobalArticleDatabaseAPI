import { Component, OnInit, HostListener, ViewChild } from '@angular/core';
import { ApplicationService } from 'src/app/services/gadb.service';
import { Article } from '../models/article';
import { TdLoadingService } from '@covalent/core/loading';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs';

import { startWith, map } from 'rxjs/operators';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ArticleComponent } from '../article/article.component';
import { ArticleContext } from '../models/article-context';
import { AuthenticationService } from '../services/auth.service';
import { ActivatedRoute } from '@angular/router';
import { WorkspaceEntry } from '../models/workspace.entry';
import { MatSelectionListChange, MatSelectionList } from '@angular/material/list';

@Component({
  selector: 'articles',
  templateUrl: './articles.component.html',
  styleUrls: ['./articles.component.scss']
})
export class ArticlesComponent implements OnInit {

  constructor(
    private appService: ApplicationService,
    private loadingService: TdLoadingService,
    private _iconRegistry: MatIconRegistry, 
    private _domSanitizer:DomSanitizer,
    public dialog: MatDialog,
    private authService: AuthenticationService,
    private route: ActivatedRoute) {
    this._iconRegistry.addSvgIconInNamespace('assets', 'gadb',
    this._domSanitizer.bypassSecurityTrustResourceUrl('assets/gadb.svg'));
  }
  
  @ViewChild('workspaceOptions') workspaceOptions : MatSelectionList;
  
  authorControl = new FormControl();
  categoryControl = new FormControl();
  topicControl = new FormControl();
  ownerControl = new FormControl();
  sourceControl = new FormControl();
  sortControl = new FormControl();

  PAGE_SIZE = 25;
  searchText = '';
  currentPage = 1;
  showMore = false;
  authors = [];
  categories = [];
  topics = [];
  owners = [];
  total = 0;
  username = '';

  sortFilters = ['Newest', 'Oldest']

  reviewedBy: string;
  status: string;

  articles: Article[] = [];

  workspace: WorkspaceEntry[];
  translationStatus: string[];

  filteredAuthors: Observable<string[]>;
  filteredCategories: Observable<string[]>;
  filteredTopics: Observable<string[]>;
  filteredSources: Observable<string[]>;
  filteredOwners: Observable<string[]>;
 
  ngOnInit(): void {
    this.username = this.authService.currentToken().username;
    this.sortControl.setValue(this.sortFilters[0]);
    this.route.queryParamMap.subscribe(params => {
      let articleId = params.get('article');
      if (articleId) {
        this.appService.getArticle(articleId)
        .subscribe(data => {
          this.openArticle(data);
        });
      }
    });

    this.appService.getAuthors()
      .subscribe(data => {
        this.authors = data.items.sort();
          
        this.filteredAuthors = this.authorControl.valueChanges
          .pipe(
            startWith(''),
            map(value => this._filter(value, this.authors))
          );
    });

    this.appService.getCategories()
      .subscribe(data => {
        this.categories = data.items.sort();
          
        this.filteredCategories = this.categoryControl.valueChanges
          .pipe(
            startWith(''),
              map(value => this._filter(value, this.categories))
            );
      });

    this.appService.getTopics()
      .subscribe(data => {
        this.topics = data.items.sort();
        
        this.filteredTopics = this.topicControl.valueChanges
          .pipe(
            startWith(''),
            map(value => this._filter(value, this.topics))
          );
    });

    this.appService.getOwners()
      .subscribe(data => {
        this.owners = data.items.sort();
      
        this.filteredOwners = this.ownerControl.valueChanges
          .pipe(
            startWith(''),
            map(value => this._filter(value, this.owners))
          );
    });

    this.appService.getWorkspace()
      .subscribe(data => {
        this.workspace = data.workspaceEntries;
      });

    this.appService.getTranslationStatus()
      .subscribe(data => {
        this.translationStatus = data;
      });

      this.search();
  }

  private _filter(value: string, values: string[]): string[] {
    const filterValue = value.toLowerCase();
    return values.filter(option => {
      if (option) return option.toLowerCase().includes(filterValue);
    });
  }

  filterChanged() {
    this.clearResults();
    this.search();
  }

  search() {
    this.showMore = false;
    this.loadingService.register("loading");
    this.appService.getArticles({
      page : this.currentPage,
      pageSize: this.PAGE_SIZE,
      text: this.searchText,
      author: this.authorControl.value,
      category: this.categoryControl.value,
      topic: this.topicControl.value,
      owner: this.ownerControl.value,
      sortBy: this.sortFilters.indexOf(this.sortControl.value),
      reviewedBy: this.reviewedBy,
      status: this.status
    })
      .subscribe(data => {
        this.total = data.total;
        this.articles = this.articles.concat(data.articles);
        this.loadingService.resolve("loading");
        this.showMore = (this.currentPage * this.PAGE_SIZE) < data.total;
      });
  }

  filterByWorkspace(event: MatSelectionListChange) {
    if (event.option.selected) {
      this.workspaceOptions.deselectAll();
      this.workspaceOptions.selectedOptions.select(event.option);
      this.status = event.option.value.status != 'All Asigned To Me' ? event.option.value.status : null;
      this.reviewedBy = event.option.value.reviewer;
    } else {
      this.status = null;
      this.reviewedBy = null;
    }

    this.clearResults();
    this.search();
  }

  textChange() {
    this.clearResults();
    this.search();
  }

  loadMore() {
    this.currentPage++;
    this.search();
  }

  clearResults() {
    this.currentPage = 1;
    this.articles = [];
  }

  openArticle(article: Article) {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.data = {
      article: article,
      authors: this.authors,
      categories: this.categories,
      owners: this.owners,
      topics: this.topics,
      status: this.translationStatus
    };
    const dialogRef = this.dialog.open(ArticleComponent, dialogConfig);

    dialogRef.updateSize("100vw");

    dialogRef.afterClosed().subscribe((data: ArticleContext) => {
      
      var allPromises = [];

      if (data) {
        const updateArticle = JSON.stringify(data.article) != JSON.stringify(data.articleUpdated);
        
        if (updateArticle) {
          allPromises.push(this.appService.updateArticle(data.articleUpdated).toPromise());
        }

        if (data.text != data.textUpdated) {
          allPromises.push(this.appService.updateArticleText(data.articleUpdated.id, data.textUpdated).toPromise());
        }
        
        data.article.translations.forEach(item => {

          if (data.translationTextUpdated[item.language]) {
            item.hasText = true;
          }

          if (JSON.stringify(item) != JSON.stringify(data.articleUpdated.translations.find(s => s.language == item.language))) {
            allPromises.push(this.appService.updateTranslation(data.articleUpdated.id, data.articleUpdated.translations.find(s => s.language == item.language)).toPromise());
          }

          if (data.translationText[item.language] != data.translationTextUpdated[item.language]) {
            allPromises.push(this.appService.updateTranslationText(data.articleUpdated.id, data.translationTextUpdated[item.language], item.language).toPromise());
          }

        });

        if (allPromises.length > 0) {
          Promise.all(allPromises).then(() => {
            // Retrieve article...
            this.appService.getArticle(article.id).subscribe(articleUpdated => {
              article.author = articleUpdated.author;
              article.category = articleUpdated.category;
              article.date = articleUpdated.date;
              article.hasText = articleUpdated.hasText;
              article.imageLink = articleUpdated.imageLink;
              article.language = articleUpdated.language;
              article.owner = articleUpdated.owner;
              article.sourceLink = articleUpdated.sourceLink;
              article.subtitle = articleUpdated.subtitle;
              article.summary = articleUpdated.summary;
              article.textLink = articleUpdated.textLink;
              article.title = articleUpdated.title;
              article.topics = articleUpdated.topics;
              article.translations = articleUpdated.translations;
            });
          });
        }    
      }
    });
  }

  logout() {
    this.authService.logout();
  }
}
