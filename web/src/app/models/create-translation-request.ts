import { Translation } from './translation';

export class CreateTranslationRequest {
    articleId: string;
    text: string;
    translation: Translation;
}