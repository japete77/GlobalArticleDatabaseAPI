import { Component, OnInit, Inject, ViewChild, ElementRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SelectItemDialog } from '../models/selectitem.dialog';
import { MatChipInputEvent } from '@angular/material/chips';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { FormControl } from '@angular/forms';
import { MatAutocompleteSelectedEvent, MatAutocomplete } from '@angular/material/autocomplete';
import { Observable } from 'rxjs';
import { startWith, map } from 'rxjs/operators';

@Component({
  selector: 'app-select-item',
  templateUrl: './select-item.component.html',
  styleUrls: ['./select-item.component.scss']
})
export class SelectItemComponent implements OnInit {

  readonly separatorKeysCodes: number[] = [ENTER, COMMA];

  itemsCtrl = new FormControl();ç
  @ViewChild('itemsInput') itemsInput: ElementRef<HTMLInputElement>;
  @ViewChild('auto') matAutocomplete: MatAutocomplete;

  filteredItems: Observable<string[]>;

  editedItems: string[];
  
  constructor(public dialogRef: MatDialogRef<SelectItemComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SelectItemDialog) {
      this.editedItems = data.selected.slice(0);
      this.filteredItems = this.itemsCtrl.valueChanges.pipe(
        startWith(null),
        map((item: string | null) => item ? this._filter(item) : this.data.list.slice()));
  }

  private _filter(value: string): string[] {
    const filterValue = value.toLowerCase();
    return this.data.list.filter(option => {
      if (option) return option.toLowerCase().includes(filterValue);
    });
  }

  ngOnInit(): void {
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  add(event: MatChipInputEvent): void {
    const input = event.input;
    const value = event.value;

    // Add our item
    if ((value || '').trim()) {
      this.editedItems.push(value.trim());
    }

    // Reset the input value
    if (input) {
      input.value = '';
    }
  }

  selected(event: MatAutocompleteSelectedEvent): void {
    this.editedItems.push(event.option.viewValue);
    this.itemsInput.nativeElement.value = '';
    this.itemsCtrl.setValue(null);
  }

  remove(item: string): void {
    const index = this.editedItems.indexOf(item);

    if (index >= 0) {
      this.editedItems.splice(index, 1);
    }
  }
}
