import { Article } from './article';

export class ArticleContext {
    article: Article;
    authors: string[];
    categories: string[];
    owners: string[];
    topics: string[];
}