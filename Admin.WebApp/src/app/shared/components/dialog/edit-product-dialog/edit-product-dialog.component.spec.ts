import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditProductDialogComponent } from './edit-product-dialog.component';

describe('EditProductDialogComponent', () => {
  let component: EditProductDialogComponent;
  let fixture: ComponentFixture<EditProductDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditProductDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditProductDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
