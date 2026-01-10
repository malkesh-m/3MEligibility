import { AfterViewInit, Component, Inject, OnInit } from "@angular/core";
import { AbstractControl, FormArray, FormBuilder, FormGroup, Validators, ValidationErrors } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { UtilityService } from "../../../../core/services/utility/utils";

// Custom Validator for Date Comparison
export function fromToDateValidator(control: AbstractControl): ValidationErrors | null {
  const fromValue = control.get('fromValue')?.value;
  const toValue = control.get('toValue')?.value;

  if (fromValue && toValue && fromValue > toValue) {
    return { fromToDate: true }; // Validation error key
  }
  return null; // No validation error
}

@Component({
  selector: 'app-computed-values-dialog',
  templateUrl: './computed-values-dialog.component.html',
  standalone: false
})
export class ComputedValuesDialogComponent implements OnInit {
  form!: FormGroup;
  ComputedParameterType = ComputedParameterType;
  rangeType = RangeType;
  dataTypeId: number = 0;
  constructor(
    private fb: FormBuilder,
    private utilityService: UtilityService,
    public dialogRef: MatDialogRef<ComputedValuesDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.dataTypeId = data?.dataTypeId ?? 0;
}


  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      computedValues: this.fb.array([]),
    });
    const formArray = this.form.get('computedValues') as FormArray;
    if (this.data?.computedValues && Array.isArray(this.data.computedValues) && this.data.computedValues.length > 0) {
      this.data.computedValues.forEach((value: ComputedValueModel) => { 
        const group = this.createComputedValueGroup();
        group.patchValue({
          computedParameterType: +value.computedParameterType,
          fromValue: value.fromValue,
          toValue: value.toValue,
          parameterExactValue: value.parameterExactValue,
          computedValue: value.computedValue,
          rangeType: value.rangeType !== null ? +value.rangeType! : null,
        });
        formArray.push(group);
      });
    } else {
      this.addComputedValue();
    }
  }

  get computedValues(): FormArray {
    return this.form.get('computedValues') as FormArray;
  }

  createComputedValueGroup(): FormGroup {

    const group = this.fb.group({
      computedParameterType: [ComputedParameterType.Single, Validators.required],
      fromValue: [''],
      toValue: [''],
      rangeType: [null],
      parameterExactValue: ['', Validators.required],
      computedValue: ['', Validators.required],
    });

    // Apply validation based on type selection
    group.get('computedParameterType')?.valueChanges.subscribe((type) => {

      const isSingle = Number(type ?? 0) === ComputedParameterType.Single;

      if (isSingle) {
        const validators = [Validators.required];

        if (this.dataTypeId === 2) {
          validators.push(Validators.pattern(/^\d+(\.\d+)?$/));
        }

        group.get('parameterExactValue')?.setValidators(validators);
        group.get('fromValue')?.clearValidators();
        group.get('toValue')?.clearValidators();
      }
      else {
        // RANGE MODE
        group.get('parameterExactValue')?.clearValidators();

        const numericValidators =
          this.dataTypeId === 2
            ? [Validators.required, Validators.pattern(/^\d+(\.\d+)?$/)]
            : [Validators.required];

        group.get('fromValue')?.setValidators(numericValidators);
        group.get('toValue')?.setValidators(numericValidators);
      }

      // UPDATE VALIDATORS
      group.get('parameterExactValue')?.updateValueAndValidity();
      group.get('fromValue')?.updateValueAndValidity();
      group.get('toValue')?.updateValueAndValidity();

      // 🔥 Add overlap validator dynamically only in RANGE mode
      const parentArray = this.computedValues;
      if (!isSingle && parentArray) {
        group.setValidators([
          fromToDateValidator,
          (control) => noRangeOverlapValidator(control, parentArray)
        ]);
      } else {
        group.setValidators([fromToDateValidator]);
      }

      group.updateValueAndValidity({ emitEvent: false });
    });

    return group;
  }


  addComputedValue(): void {
    this.computedValues.push(this.createComputedValueGroup());
  }

  removeComputedValue(index: number): void {
    this.computedValues.removeAt(index);
  }

  save(): void {
    this.dialogRef.close(this.computedValues.value);
  }

  isSingle(group: AbstractControl): boolean {
    const value = +group.get('computedParameterType')?.value;
    return value === ComputedParameterType.Single;
  }

  isRange(group: AbstractControl): boolean {
    const value = +group.get('computedParameterType')?.value;
    return value == ComputedParameterType.Range;
  }

  get computedParameterTypeKeys(): (keyof typeof ComputedParameterType)[] {
    return Object.keys(ComputedParameterType)
      .filter(key => isNaN(Number(key))) as (keyof typeof ComputedParameterType)[];
  }
  numericOnly(event: KeyboardEvent) {
    const allowed = ['Backspace', 'Tab', 'ArrowLeft', 'ArrowRight', 'Delete'];

    if (allowed.includes(event.key)) return;

    if (!/^[0-9.]$/.test(event.key)) {
      event.preventDefault();
    }

  }
}
export function noRangeOverlapValidator(group: AbstractControl, parentArray: FormArray): ValidationErrors | null {
  const from = +group.get('fromValue')?.value;
  const to = +group.get('toValue')?.value;

  // Skip if values are missing or invalid
  if (isNaN(from) || isNaN(to)) return null;

  // Iterate other groups and compare
  for (let control of parentArray.controls) {
    if (control === group) continue; // skip itself

    const otherFrom = +control.get('fromValue')?.value;
    const otherTo = +control.get('toValue')?.value;

    if (!isNaN(otherFrom) && !isNaN(otherTo)) {
      const overlaps =
        (from >= otherFrom && from <= otherTo) ||
        (to >= otherFrom && to <= otherTo) ||
        (from <= otherFrom && to >= otherTo);

      if (overlaps) {
        return { rangeOverlap: true };
      }
    }
  }

  return null;
}

export interface ComputedValueModel {
  computedParameterType: ComputedParameterType;
  fromValue: string;
  toValue: string;
  rangeType?: RangeType;
  parameterExactValue: string;
  computedValue: string;
}

export enum ComputedParameterType {
  Single = 0,
  Range = 1
}

export enum RangeType {
  Hours = 0,
  Days = 1,
}
