import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-product-remove',
    standalone: true,
    imports: [CommonModule],
    templateUrl: 'remove-product.component.html',
    styleUrls: ['remove-product.component.scss']
})
export class RemoveProductComponent implements OnInit {
    constructor() { }

    ngOnInit() { }
}
