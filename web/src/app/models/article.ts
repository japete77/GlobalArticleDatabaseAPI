import { Translation } from './translation';

export class Article {
    id: string;
    date: Date;
    author: string[];
    category: string;
    topics: string[];
    title: string;
    subtitle: string;
    summary: string;
    language: string;
    sourceLink: string;
    words: number;
    characters: number;
    owner: string;
    hasText: boolean;
    textLink: string;
    imageLink: string;
    translations: Translation[];
}