import { Publication } from './publication';

export class Translation {
    title: string;
    subtitle: string;
    summary: string;
    date: Date;
    language: string;
    status: string;
    translatedBy: string;
    reviewedBy: string;
    hasText: boolean;
    textLink: string;
    publications: Publication[];
}