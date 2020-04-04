import { Article } from './article';

export class ArticleSearchResponse {
    total: number;
    currentPage: number;
    articles: Article[];
}