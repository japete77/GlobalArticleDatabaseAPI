import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { GetListResponse } from 'src/app/models/get-list-response';
import { ArticleSearchResponse } from 'src/app/models/article-search-response';
import { Article } from 'src/app/models/article';
import { UpdateArticleRequest } from 'src/app/models/update-article-request';
import { UpdateArticleTextRequest } from 'src/app/models/update-article-text-request';
import { Translation } from 'src/app/models/translation';
import { UpdateTranslationRequest } from 'src/app/models/update-translation-request';
import { identifierModuleUrl } from '@angular/compiler';
import { UpdateTranslationTextRequest } from 'src/app/models/update-translation-text-request';

@Injectable({ providedIn: "root" })
export class ApplicationService {

  constructor(private http: HttpClient) {}

  getAuthors() {
    return this.http.get<GetListResponse>(
      `${environment.gadb_api_base}reference-data/authors`
    );
  }

  getCategories() {
    return this.http.get<GetListResponse>(
      `${environment.gadb_api_base}reference-data/categories`
    );
  }

  getTopics() {
    return this.http.get<GetListResponse>(
      `${environment.gadb_api_base}reference-data/topics`
    );
  }

  getOwners() {
    return this.http.get<GetListResponse>(
      `${environment.gadb_api_base}reference-data/owners`
    );
  }

  getArticles(page: number, pageSize: number, text?: string, author?: string, category?: string, topic?: string, owner?: string, sortByAuthor?: number,  sortByDate?: number,  sortBySource?: number) {
    
    let query = `?page=${page}&pageSize=${pageSize}`;
    if (text) {
      query += `&Text=${text}`;
    }

    if (author) {
      query += `&author=${author}`;
    }

    if (category) {
      query += `&category=${category}`;
    }

    if (topic) {
      query += `&topic=${topic}`;
    }

    if (owner) {
      query += `&owner=${owner}`;
    }

    if (sortByDate) {
      if (sortByDate == 0) {
        query += `&ByDateAsc=true`;
      } else {
        query += `&ByDateAsc=false`;
      }
    }

    if (sortByAuthor) {
      if (sortByAuthor == 2) {
        query += `&ByAuthorAsc=true`;
      } else {
        query += `&ByAuthorAsc=false`;
      }
    }

    if (sortBySource) {
      if (sortBySource == 4) {
        query += `&ByOwnerAsc=true`;
      } else {
        query += `&ByOwnerAsc=false`;
      }
    }

    return this.http.get<ArticleSearchResponse>(
      `${environment.gadb_api_base}articles${query}`
    );
  }

  getArticleText(id: string, language?: string) {
    var url = `${environment.gadb_api_base}article/${id}/text`
    if (language) url += `?language=${language}`;
    return this.http.get<string>(url);
  }

  updateArticle(article: Article) {
    var body: UpdateArticleRequest = { article: article };
    return this.http.put(
      `${environment.gadb_api_base}article`,
      body
    );
  }

  updateArticleText(id: string, text: string) {
    var body: UpdateArticleTextRequest = { id: id, text: text };
    return this.http.put(
      `${environment.gadb_api_base}article/text`,
      body
    );
  }

  updateTranslation(articleId: string, translation: Translation) {
    var body: UpdateTranslationRequest = { articleId: articleId, translation: translation };
    return this.http.put(
      `${environment.gadb_api_base}translation`,
      body
    );
  }

  updateTranslationText(articleId: string, text: string, language: string) {
    var body: UpdateTranslationTextRequest = { articleId: articleId, text: text, language: language };
    return this.http.put(
      `${environment.gadb_api_base}translation/text`,
      body
    );
  }
}