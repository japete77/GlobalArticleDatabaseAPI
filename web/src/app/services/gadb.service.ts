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
import { UpdateTranslationTextRequest } from 'src/app/models/update-translation-text-request';
import { AppConfig } from 'src/app/helpers/app-config';
import { WorkspaceResponse } from 'src/app/models/responses/workspace.response';
import { ArticleFilter } from '../models/article.filter';

@Injectable({ providedIn: "root" })
export class ApplicationService {

  constructor(private http: HttpClient) {}

  getAuthors() {
    return this.http.get<GetListResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/reference-data/authors`
    );
  }

  getCategories() {
    return this.http.get<GetListResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/reference-data/categories`
    );
  }

  getTopics() {
    return this.http.get<GetListResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/reference-data/topics`
    );
  }

  getOwners() {
    return this.http.get<GetListResponse>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/reference-data/owners`
    );
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

  updateTranslation(articleId: string, translation: Translation) {
    var body: UpdateTranslationRequest = { articleId: articleId, translation: translation };
    return this.http.put(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/translation`,
      body
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

  getTranslationStatus() {
    return this.http.get<string[]>(
      `${AppConfig.settings.api_base_url}${AppConfig.settings.api_version}/translation/status`,      
    );
  }
}