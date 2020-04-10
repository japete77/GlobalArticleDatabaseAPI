import { User } from '../user.model';

export class SearchUserResponse {
    currentPage: number;
    total: number;
    users: User[];
    next: string;
}