import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GetListResponse } from 'src/app/models/get-list-response';
import { ArticleSearchResponse } from 'src/app/models/article-search-response';
import { Article } from 'src/app/models/article';
import { UpdateArticleRequest } from 'src/app/models/update-article-request';
import { UpdateArticleTextRequest } from 'src/app/models/update-article-text-request';
import { Translation } from 'src/app/models/translation';
import { UpdateTranslationRequest } from 'src/app/models/update-translation-request';
import { UpdateTranslationTextRequest } from 'src/app/models/update-translation-text-request';
import { AppConfig } from 'src/app/helpers/app-config';
import { WorkspaceResponse } from 'src/app/models/responses/workspace.response';
import { ArticleFilter } from '../models/article.filter';
import { CreateTranslationRequest } from '../models/create-translation-request';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({ providedIn: "root" })
export class ApplicationService {

  private searchResponse = new BehaviorSubject<ArticleSearchResponse>(null)
  private searchRequest = new BehaviorSubject<ArticleFilter>(null)
  private totalResultsResponse = new BehaviorSubject<number>(null)
  private initialized = new BehaviorSubject<boolean>(null)
  private currentFilter: ArticleFilter = { page: 0, pageSize: 25 }
  readonly PAGE_SIZE = 25

  authors: string[]
  categories: string[]
  topics: string[]
  owners: string[]
  translationLanguages: string[]
  translationStatuses: string[]
  sortedBy: string[] = [ "Newest", "Oldest" ]

  constructor(private http: HttpClient) {
    var allPromises = []

    allPromises.push(this.refreshAuthors())
    allPromises.push(this.refreshCategories())
    allPromises.push(this.refreshTopics())
    allPromises.push(this.refreshOwners())
    allPromises.push(this.refreshTranslationLanguages())
    allPromises.push(this.refreshTranslationStatuses())

    Promise.all(allPromises).then(results => {
      var i = 0;
      this.authors = results[i++].items
      this.categories = results[i++].items.filter(item => item && item.trim().length > 0)
      this.topics = results[i++].items.filter(item => item && item.trim().length > 0)
      this.owners = results[i++].items.filter(item => item && item.trim().length > 0)
      this.translationLanguages = results[i++].items.filter(item => item && item.trim().length > 0)
      this.translationStatuses = results[i++].filter(item => item && item.trim().length > 0)
      this.initialized.next(true)
    });
  }

  getSearchResponse() : Observable<ArticleSearchResponse> {
    return this.searchResponse.asObservable()
  }

  getSearchRequest() : Observable<ArticleFilter> {
    return this.searchRequest.asObservable()
  }

  getTotalResultsResponse() : Observable<number> {
    return this.totalResultsResponse.asObservable()
  }

  getInitialized() : Observable<boolean> {
    return this.initialized.asObservable()
  }

  search(filter: ArticleFilter) {
    if (filter) {
      this.currentFilter = filter
    }
    this.currentFilter.page = 0
    this.currentFilter.pageSize = this.PAGE_SIZE
    this.loadMore()
  }

  loadMore() {
    this.currentFilter.page++
    this.searchRequest.next(this.currentFilter);
    this.getArticles(this.currentFilter).subscribe(data => {
      this.searchResponse.next(data)
      this.totalResultsResponse.next(data.total)
    })
  }

  refreshAuthors() : Promise<GetListResponse> {
    return this.http.get<GetListResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/reference-data/authors`
    ).toPromise()
  }

  getAuthors() : string[] {
    return this.authors
  }

  refreshCategories() : Promise<GetListResponse> {
    return this.http.get<GetListResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/reference-data/categories`
    ).toPromise()
  }

  getCategories() : string[] {
    return this.categories
  }

  refreshTopics() : Promise<GetListResponse> {
    return this.http.get<GetListResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/reference-data/topics`
    ).toPromise()
  }

  getTopics() {
    return this.topics
  }

  refreshOwners() : Promise<GetListResponse> {
    return this.http.get<GetListResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/reference-data/owners`
    ).toPromise()
  }

  getOwners() : string[] {
    return this.owners
  }

  refreshTranslationLanguages() : Promise<GetListResponse> {
    return this.http.get<GetListResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/reference-data/translation-languages`
    ).toPromise()
  }

  getTranslationLanguages() : string[] {
    return this.translationLanguages
  }

  refreshTranslationStatuses() : Promise<string[]> {
    return this.http.get<string[]>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/translation/status`
    ).toPromise()
  }

  getTranslationStatuses() : string[] {
    return this.translationStatuses
  }

  getSortedBy() : string[] {
    return this.sortedBy
  }

  getArticle(id: string) {
    return this.http.get<Article>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/article/${id}`
    );
  }

  getArticles(filter: ArticleFilter) {
    
    let query = `?page=${filter.page}&pageSize=${filter.pageSize}`;
    if (filter.text) {
      
      query += `&Text=${encodeURIComponent(filter.text.replace(/&amp;/g, "&"))}`;
    }

    if (filter.author) {
      query += `&author=${encodeURIComponent(filter.author.replace(/&amp;/g, "&"))}`;
    }

    if (filter.category) {
      query += `&category=${encodeURIComponent(filter.category.replace(/&amp;/g, "&"))}`;
    }

    if (filter.topic) {
      query += `&topic=${encodeURIComponent(filter.topic.replace(/&amp;/g, "&"))}`;
    }

    if (filter.owner) {
      query += `&owner=${encodeURIComponent(filter.owner.replace(/&amp;/g, "&"))}`;
    }

    if (filter.reviewedBy) {
      query += `&reviewedBy=${encodeURIComponent(filter.reviewedBy.replace(/&amp;/g, "&"))}`;
    }

    if (filter.status) {
      query += `&status=${encodeURIComponent(filter.status.replace(/&amp;/g, "&"))}`;
    }

    if (filter.translationLanguage) {
      query += `&translationLanguage=${encodeURIComponent(filter.translationLanguage.replace(/&amp;/g, "&"))}`;
    }

    if (filter.sortBy == 0) {
      query += `&ByDateAsc=false`;
    } else if (filter.sortBy == 1) {
      query += `&ByDateAsc=true`;
    }

    return this.http.get<ArticleSearchResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/articles${query}`
    );
  }

  getArticleText(id: string, language?: string) {
    var url = `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/article/${id}/text`
    if (language) url += `?language=${language}`;
    return this.http.get<string>(url);
  }

  updateArticle(article: Article) {
    var body: UpdateArticleRequest = { article: article };
    return this.http.put(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/article`,
      body
    );
  }

  updateArticleText(id: string, text: string) {
    var body: UpdateArticleTextRequest = { id: id, text: text };
    return this.http.put(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/article/text`,
      body
    );
  }

  createTranslation(articleId: string, text: string, translation: Translation) {
    var body: CreateTranslationRequest = { articleId: articleId, text: text, translation: translation };
    return this.http.post(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/translation`,
      body
    );
  }

  updateTranslation(articleId: string, translation: Translation) {
    var body: UpdateTranslationRequest = { articleId: articleId, translation: translation };
    return this.http.put(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/translation`,
      body
    );
  }

  deleteTranslation(articleId: string, language: string) {
    return this.http.delete(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/translation?articleId=${articleId}&language=${language}`
    );
  }

  updateTranslationText(articleId: string, text: string, language: string) {
    var body: UpdateTranslationTextRequest = { articleId: articleId, text: text, language: language };
    return this.http.put(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/translation/text`,
      body
    );
  }

  getWorkspace() {
    return this.http.get<WorkspaceResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/workspace`,      
    );
  }

  getWorkspaceArticles(url: string, page: number, pageSize: number) {
    return this.http.get<ArticleSearchResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/${url}&page=${page}&pageSize=${pageSize}`
    );
  }
}