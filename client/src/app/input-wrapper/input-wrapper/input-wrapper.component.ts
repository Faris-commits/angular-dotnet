import { Component, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-input-wrapper',
  standalone: true,
  imports: [],
  templateUrl: './input-wrapper.component.html',
  styleUrls: ['./input-wrapper.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputWrapperComponent),
      multi: true,
    },
  ]
})
export class InputWrapperComponent {
  @Input() label: string = '';
  @Input() type: string = 'text';
  @Input() name: string = '';

  value: any = '';

  onChange = (value: any) => {};
  onTouched = () => {};

  writeValue(value: any): void {
    this.value = value;
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  handleInputChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.value = input.value;
    this.onChange(this.value);
  }
}
