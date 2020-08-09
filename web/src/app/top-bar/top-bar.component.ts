import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { ENTER, COMMA } from '@angular/cdk/keycodes';
import { AuthenticationService } from '../services/auth.service';
import { ApplicationService } from '../services/gadb.service';
import { ArticleFilter } from '../models/article.filter';
import { Observable } from 'rxjs';
import { FormControl } from '@angular/forms';
import { startWith, map } from 'rxjs/operators';
import { MatAutocompleteSelectedEvent, MatAutocomplete, MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { CookieService } from '../services/cookie.service';

@Component({
  selector: 'top-bar',
  templateUrl: './top-bar.component.html',
  styleUrls: ['./top-bar.component.scss']
})
export class TopBarComponent implements OnInit {

  @ViewChild('search') searchElement: ElementRef;
  @ViewChild('search', { read: MatAutocompleteTrigger }) autoComplete: MatAutocompleteTrigger;

  readonly TEXT = 0
  readonly AUTHOR = 1
  readonly CATEGORY = 2
  readonly SOURCE = 3
  readonly TOPIC = 4
  readonly TRANSLATION_LANGUAGE = 5
  readonly TRANSLATION_STATUS = 6
  readonly SORTED = 7
  readonly REVIEWED_BY_ME = 8

  readonly SEARCH_FILTER_KEY = "gadb_search_filter"

  visible = true
  selectable = true
  removable = true
  addOnBlur = true
  username = ""
  total = 0
  termCtrl = new FormControl()
  filteredTerms: Observable<string[]>
  allTerms: string[] = []
  readonly separatorKeysCodes: number[] = [ENTER, COMMA]
  searchItems = []
  selectedTermType = this.TEXT
  searchFilter: ArticleFilter = {
    page: 0,
    pageSize: 0
  }
  filterSelected = false
  searchPlaceholder = "Search..."

  menuAuthorDisabled = true
  menuCategoryDisabled = true
  menuSourceDisabled = true
  menuTopicDisabled = true
  menuTranslationLanguageDisabled = true
  menuTranslationStatusDisabled = true

  constructor(
    private authService: AuthenticationService,
    private appService: ApplicationService,
    private cookieService: CookieService
  ) {
  }

  private _filter(value: string): string[] {
    const filterValue = value.toLowerCase()
    return this.allTerms.filter(term => term.toLowerCase().includes(filterValue))
  }

  ngOnInit(): void {
    this.username = this.authService.currentToken().username
    this.appService.getTotalResultsResponse().subscribe(data => {
      this.total = data
    });

    this.filteredTerms = this.termCtrl.valueChanges.pipe(
      startWith(null),
      map((term: string | null) => term ? this._filter(term) : this.allTerms ? this.allTerms.slice() : []));

    // Restore search items
    var searchItems = this.cookieService.getCookie(this.SEARCH_FILTER_KEY)
    if (searchItems) {
      this.searchItems = JSON.parse(searchItems)
    }

    this.search()
  }

  selected(event: MatAutocompleteSelectedEvent): void {

    var existingTerm = this.searchItems.find(item => item.type == this.selectedTermType)

    if (!existingTerm) {
      this.searchItems.push({
        name: event.option.viewValue,
        type: this.selectedTermType
      })
    } else {
      existingTerm.name = event.option.viewValue
    }

    this.search()

    this.searchElement.nativeElement.value = ''
    this.termCtrl.setValue(null)
    this.fireAutocomplete(null)
    this.filterSelected = true
  }

  remove(item: any): void {
    const index = this.searchItems.indexOf(item)

    if (index >= 0) {
      this.searchItems.splice(index, 1)
      this.search()
    }
  }

  search() {
    // create search filter
    this.searchFilter = {
      page: 0,
      pageSize: 0
    }

    var tokens = []
    this.searchItems.forEach(item => {
      if (item.type == this.AUTHOR) {
        this.searchFilter.author = item.name
      } else if (item.type == this.CATEGORY) {
        this.searchFilter.category = item.names
      } else if (item.type == this.SOURCE) {
        this.searchFilter.owner = item.name
      } else if (item.type == this.TOPIC) {
        this.searchFilter.topic = item.name
      } else if (item.type == this.TRANSLATION_STATUS) {
        this.searchFilter.status = item.name
      } else if (item.type == this.TRANSLATION_LANGUAGE) {
        this.searchFilter.translationLanguage = item.name
      } else if (item.type == this.SORTED) {
        this.searchFilter.sortBy = item.name == "Newest" ? 0 : 1
      } else if (item.type == this.REVIEWED_BY_ME) {
        this.searchFilter.reviewedBy = this.authService.currentToken().username
      } else if (item.type == this.TEXT) {
        var cleanToken: string = item.name.trim()
        if (cleanToken.startsWith("-")) tokens.push(cleanToken);
        else tokens.push(`"${cleanToken}"`)
      }
    })

    if (tokens.length > 0) {
      this.searchFilter.text = tokens.join(' ')
    }

    // Save search filter
    this.cookieService.setCookie(this.SEARCH_FILTER_KEY, JSON.stringify(this.searchItems), 7)

    // Call search
    this.appService.search(this.searchFilter)
  }

  keyPress(event: KeyboardEvent) {
    if (event.key == "a" && event.altKey) {
      this.fireAutocomplete(this.AUTHOR)
    }
    else if (event.key.toLowerCase() == "c" && event.altKey) {
      this.fireAutocomplete(this.CATEGORY)
    }
    else if (event.key.toLowerCase() == "s" && event.altKey) {
      this.fireAutocomplete(this.SOURCE)
    }
    else if (event.key.toLowerCase() == "t" && event.altKey) {
      this.fireAutocomplete(this.TOPIC)
    }
    else if (event.key.toLowerCase() == "u" && event.altKey) {
      this.fireAutocomplete(this.TRANSLATION_STATUS)
    }
    else if (event.key.toLowerCase() == "l" && event.altKey) {
      this.fireAutocomplete(this.TRANSLATION_LANGUAGE)
    }
    else if (event.key.toLowerCase() == "o" && event.altKey) {
      this.fireAutocomplete(this.SORTED)
    }
    else if (event.keyCode == 27) {
      // ESC - clean up
      this.fireAutocomplete(null);
    }
    else if (event.keyCode == 13 && this.selectedTermType == this.TEXT) {

      if (!this.termCtrl.value) return

      if (this.filterSelected) {
        this.filterSelected = false
        return
      }

      this.searchItems.push({
        name: this.termCtrl.value,
        type: this.selectedTermType
      })

      this.search()

      this.searchElement.nativeElement.value = ''
      this.termCtrl.setValue(null)
      this.fireAutocomplete(null)
    }

    event.stopPropagation()
  }

  onBlur() {
    this.fireAutocomplete(null);
  }

  existSearchTerm(type: number): boolean {
    return this.searchItems.findIndex(item => item.type == type) > -1
  }

  fireAutocomplete(type: number) {
    switch (type) {
      case this.AUTHOR:
        this.allTerms = this.appService.getAuthors()
        this.selectedTermType = this.AUTHOR
        this.searchPlaceholder = "By Author..."
        break
      case this.CATEGORY:
        this.allTerms = this.appService.getCategories()
        this.selectedTermType = this.CATEGORY
        this.searchPlaceholder = "By Category..."
        break
      case this.SOURCE:
        this.allTerms = this.appService.getOwners()
        this.selectedTermType = this.SOURCE
        this.searchPlaceholder = "By Source..."
        break
      case this.TOPIC:
        this.allTerms = this.appService.getTopics()
        this.selectedTermType = this.TOPIC
        this.searchPlaceholder = "By Topic..."
        break
      case this.TRANSLATION_LANGUAGE:
        this.allTerms = this.appService.getTranslationLanguages()
        this.selectedTermType = this.TRANSLATION_LANGUAGE
        this.searchPlaceholder = "By Translation Language..."
        break
      case this.TRANSLATION_STATUS:
        this.allTerms = this.appService.getTranslationStatuses()
        this.selectedTermType = this.TRANSLATION_STATUS
        this.searchPlaceholder = "By Translation Status..."
        break
      case this.SORTED:
        this.allTerms = this.appService.getSortedBy()
        this.selectedTermType = this.SORTED
        this.searchPlaceholder = "Sorted By..."
        break
      default:
        this.allTerms = []
        this.selectedTermType = this.TEXT
        this.searchPlaceholder = "Search..."
        break
    }

    this.filteredTerms = this.termCtrl.valueChanges.pipe(
      startWith(null),
      map((term: string | null) => term ? this._filter(term) : this.allTerms ? this.allTerms.slice() : [])
    );

    this.autoComplete.openPanel()
  }

  byAuthor() {
    setTimeout(() => {
      this.fireAutocomplete(this.AUTHOR)
      this.searchElement.nativeElement.focus()
    }, 0)
  }

  byCategory() {
    setTimeout(() => {
      this.fireAutocomplete(this.CATEGORY)
      this.searchElement.nativeElement.focus()
    }, 0)
  }

  bySource() {
    setTimeout(() => {
      this.fireAutocomplete(this.SOURCE)
      this.searchElement.nativeElement.focus()
    }, 0)
  }

  byTopic() {
    setTimeout(() => {
      this.fireAutocomplete(this.TOPIC)
      this.searchElement.nativeElement.focus()
    }, 0)
  }

  byLanguage() {
    setTimeout(() => {
      this.fireAutocomplete(this.TRANSLATION_LANGUAGE)
      this.searchElement.nativeElement.focus()
    }, 0)
  }

  byStatus() {
    setTimeout(() => {
      this.fireAutocomplete(this.TRANSLATION_STATUS)
      this.searchElement.nativeElement.focus()
    }, 0)
  }

  sortBy() {
    setTimeout(() => {
      this.fireAutocomplete(this.SORTED)
      this.searchElement.nativeElement.focus()
    }, 0)
  }

  assignedToMe() {
    var existingTerm = this.searchItems.find(item => item.type == this.REVIEWED_BY_ME)

    if (!existingTerm) {
      this.searchItems.push({
        name: "Reviewed by me",
        type: this.REVIEWED_BY_ME
      })
  
      this.search()  
    }
  }

  logout() {
    this.authService.logout();
  }
}
