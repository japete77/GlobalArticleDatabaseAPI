import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { LanguagesDialog } from '../models/languages.dialog';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { startWith, map } from 'rxjs/operators';

@Component({
  selector: 'app-select-language',
  templateUrl: './select-language.component.html',
  styleUrls: ['./select-language.component.scss']
})
export class SelectLanguageComponent implements OnInit {

  selectedLanguage = '';
  languagesControl = new FormControl();
  filteredLanguages : Observable<string[]>;

  constructor(public dialogRef: MatDialogRef<SelectLanguageComponent>,
    @Inject(MAT_DIALOG_DATA) public data: LanguagesDialog) { }

  ngOnInit(): void {
    this.filteredLanguages = this.languagesControl.valueChanges
    .pipe(
      startWith(''),
      map(value => this._filter(value, this.data.languages))
    );
  } 

  onCancel(): void {
    this.dialogRef.close();
  }

  private _filter(value: string, values: string[]): string[] {
    const filterValue = value.toLowerCase();
    return values.filter(option => {
      if (option) return option.toLowerCase().includes(filterValue);
    });
  }

}
