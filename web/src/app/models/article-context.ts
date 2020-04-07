import { Article } from './article';

export class ArticleContext {
    authors: string[];
    categories: string[];
    owners: string[];
    topics: string[];
    article: Article;
    articleUpdated: Article;
    text: string;
    textUpdated: string;
    translationText: any;
    translationTextUpdated: any;
}