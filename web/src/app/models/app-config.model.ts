export interface IAppConfig {
    production: boolean;
    api_base_url: string;
    s3_url: string;
    app_renew_before_mins: number;
    api_version: string;
}