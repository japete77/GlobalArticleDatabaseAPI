import { Translation } from './translation';

export class Article {
    id: string;
    date: Date;
    author: string;
    category: string;
    topics: string[];
    title: string;
    subtitle: string;
    summary: string;
    language: string;
    sourceLink: string;
    owner: string;
    hasText: boolean;
    textLink: string;
    hasImage: boolean;
    imageLink: string;
    translations: Translation[];
}