import { Component, OnInit, HostListener } from '@angular/core';
import { ApplicationService } from 'src/services/gadb.service';
import { Article } from './models/article';
import { TdLoadingService } from '@covalent/core/loading';
import { MatRadioChange } from '@angular/material/radio';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs';

import { startWith, map } from 'rxjs/operators';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ArticleComponent } from './article/article.component';
import { ArticleContext } from './models/article-context';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  constructor(
    private appService: ApplicationService,
    private loadingService: TdLoadingService,
    private _iconRegistry: MatIconRegistry, 
    private _domSanitizer:DomSanitizer,
    public dialog: MatDialog) {
    this._iconRegistry.addSvgIconInNamespace('assets', 'gadb',
    this._domSanitizer.bypassSecurityTrustResourceUrl('assets/gadb.svg'));
  }
  
  authorControl = new FormControl();
  categoryControl = new FormControl();
  topicControl = new FormControl();
  ownerControl = new FormControl();
  sourceControl = new FormControl();

  PAGE_SIZE = 25;
  searchText = '';
  currentPage = 1;
  showMore = false;
  authors = []; // [ 'John Piper', 'Tony Reinke', 'David Platt', 'Mark Dever', 'Donald Carson', 'Yancey', 'Miguel Núñez', 'Paul Washer' ];
  categories = [];
  topics = [];
  owners = [];

  sortByDate : number;
  sortByAuthor : number;
  sortBySource : number;

  articles: Article[] = [];

  filteredAuthors: Observable<string[]>;
  filteredCategories: Observable<string[]>;
  filteredTopics: Observable<string[]>;
  filteredSources: Observable<string[]>;
  filteredOwners: Observable<string[]>;
 
  ngOnInit(): void {
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

    this.search();
  }

  private _filter(value: string, values: string[]): string[] {
    const filterValue = value.toLowerCase();
    return values.filter(option => option.toLowerCase().includes(filterValue));
  }

  filterChanged() {
    this.clearResults();
    this.search();
  }

  search() {
    this.showMore = false;
    this.loadingService.register("loading");
    this.appService.getArticles(
        this.currentPage, 
        this.PAGE_SIZE, 
        this.searchText, 
        this.authorControl.value, 
        this.categoryControl.value,
        this.topicControl.value,
        this.ownerControl.value,
        this.sortByAuthor,
        this.sortByDate, 
        this.sortBySource
      )
      .subscribe(data => {
        this.articles = this.articles.concat(data.articles);
        this.loadingService.resolve("loading");
        this.showMore = (this.currentPage * this.PAGE_SIZE) < data.total;
      });
  }

  sortChange(event: MatRadioChange) {
    if (event.value == 0 || event.value == 1) {
      this.sortByAuthor = undefined;
      this.sortBySource = undefined;
    }

    if (event.value == 2 || event.value == 3) {
      this.sortByDate = undefined;
      this.sortBySource = undefined;
    }

    if (event.value == 4 || event.value == 5) {
      this.sortByDate = undefined;
      this.sortByAuthor = undefined;
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
      topics: this.topics
    };
    const dialogRef = this.dialog.open(ArticleComponent, dialogConfig);

    dialogRef.updateSize("80vw");

    dialogRef.afterClosed().subscribe(result => {
      console.log(`Dialog result: ${result}`);
    });
  }
}
