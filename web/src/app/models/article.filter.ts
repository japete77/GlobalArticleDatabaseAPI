export class ArticleFilter {
    page: number;
    pageSize: number;
    text?: string;
    author?: string;
    category?: string;
    topic?: string;
    owner?: string;
    sortBy?: number;
    reviewedBy?: string;
    status?: string;
}