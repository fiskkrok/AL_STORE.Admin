import { Component, OnInit } from "@angular/core";
import { FormGroup, FormBuilder, Validators } from "@angular/forms";

@Component({
    selector: 'app-variant-form',
    templateUrl: './variant-form.component.html',
})
export class VariantFormComponent implements OnInit {
    variantForm: FormGroup;

    constructor(private fb: FormBuilder) {
        this.variantForm = this.fb.group({
            sku: ['', Validators.required],
            price: [0, [Validators.required, Validators.min(0)]],
            currency: ['USD', Validators.required],
            compareAtPrice: [null],
            costPrice: [null],
            barcode: [''],
            stock: [0, [Validators.required, Validators.min(0)]],
            trackInventory: [true],
            allowBackorders: [false],
            lowStockThreshold: [null],
            sortOrder: [0],
            // Attributes will be handled separately
        });
    }
    ngOnInit(): void {
        throw new Error("Method not implemented.");
    }

    // Methods to handle form submission, etc.
}