import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';


@Component({
  selector: 'app-editor',
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class EditorComponent implements OnInit {
  sourceForm = this.fb.nonNullable.group({
    inputSource: '',
    output: ''
  });

  ngOnInit(): void {
    
  }

  constructor(private fb: FormBuilder) {}

  submitSource(): void {
    this.sourceForm.patchValue({
        output: this.sourceForm.controls.inputSource.value
    });
  }

}
