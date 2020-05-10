import { BrowserModule } from '@angular/platform-browser';
import { NgModule, CUSTOM_ELEMENTS_SCHEMA, APP_INITIALIZER } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { CovalentLayoutModule } from '@covalent/core/layout';
import { CovalentStepsModule  } from '@covalent/core/steps';
/* any other core modules */
// (optional) Additional Covalent Modules imports
import { CovalentHttpModule } from '@covalent/http';
import { CovalentHighlightModule } from '@covalent/highlight';
import { CovalentMarkdownModule } from '@covalent/markdown';
import { CovalentDynamicFormsModule } from '@covalent/dynamic-forms';

import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatFormFieldModule, MatFormField } from '@angular/material/form-field';
import { CovalentChipsModule } from '@covalent/core/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { CovalentSearchModule } from '@covalent/core/search';
import { MatCardModule } from '@angular/material/card';
import { CovalentLoadingModule } from '@covalent/core/loading';
import { DatePipe } from '@angular/common'
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatGridListModule } from '@angular/material/grid-list';
import { ArticleComponent } from './article/article.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { AppConfig } from './helpers/app-config';
import { JwtInterceptor } from './helpers/jwt.interceptor';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { RefreshTokenComponent } from './refresh-token/refresh-token.component';
import { ArticlesComponent } from './articles/articles.component';
import { LoginComponent } from './login/login.component';
import { FlexLayoutModule } from '@angular/flex-layout';
import { DialogComponent } from './dialog/dialog.component';
import { MatMenuModule } from '@angular/material/menu';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { MatListModule } from '@angular/material/list';
import { SecureDirective } from './directives/secure.directive';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { SelectLanguageComponent } from './select-language/select-language.component';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { SelectItemComponent } from './select-item/select-item.component';

export function initializeApp(appConfig: AppConfig) {
  return () => appConfig.load();
}

@NgModule({

  declarations: [
    AppComponent,
    ArticleComponent,
    RefreshTokenComponent,
    ArticlesComponent,
    LoginComponent,
    DialogComponent,
    ForgotPasswordComponent,
    ChangePasswordComponent,
    SecureDirective,
    SelectLanguageComponent,
    ConfirmDialogComponent,
    SelectItemComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    CovalentLayoutModule,
    CovalentStepsModule,
    CovalentHttpModule.forRoot(),
    CovalentHighlightModule,
    CovalentMarkdownModule,
    CovalentDynamicFormsModule,
    MatInputModule,
    MatAutocompleteModule,
    ReactiveFormsModule,
    FormsModule,
    MatFormFieldModule,
    CovalentChipsModule,
    MatExpansionModule,
    CovalentSearchModule,
    MatCardModule,
    CovalentLoadingModule,
    MatRadioModule,
    MatButtonModule,
    MatGridListModule,
    MatDialogModule,
    MatTabsModule,
    MatChipsModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    FlexLayoutModule,
    MatMenuModule,
    MatListModule,
    ClipboardModule,
    CKEditorModule
  ],
  schemas: [ 
    CUSTOM_ELEMENTS_SCHEMA 
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    DatePipe,
    AppConfig,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [AppConfig],
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
