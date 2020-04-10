export class SecurityContext {
    token?: string
    renewToken?: string
}

export class AuthToken {
    exp: number
    iat: number
    nbf: number
    role: any
    username: string
}