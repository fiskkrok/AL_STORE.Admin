// src/app/core/services/product-type.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, shareReplay, catchError } from 'rxjs/operators';
import { FormlyFieldConfig } from '@ngx-formly/core';
import { ProductType, ProductTypeAttribute } from '../../shared/models/product-type.model';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ProductTypeService {
    private readonly apiUrl = `${environment.apiUrls.admin.products}/types`;
    private cachedProductTypes: Observable<ProductType[]> | null = null;

    constructor(private http: HttpClient) { }

    getProductTypes(): Observable<ProductType[]> {
        if (!this.cachedProductTypes) {
            this.cachedProductTypes = this.http.get<ProductType[]>(this.apiUrl).pipe(
                catchError(error => {
                    console.error('Error fetching product types:', error);
                    // Fallback to local static types if API fails
                    return of(this.getStaticProductTypes());
                }),
                shareReplay(1)
            );
        }
        return this.cachedProductTypes;
    }

    getProductTypeById(id: string): Observable<ProductType | undefined> {
        return this.getProductTypes().pipe(
            map(types => types.find(type => type.id === id))
        );
    }

    getAttributeFieldsByType(typeId: string): Observable<FormlyFieldConfig[]> {
        return this.getProductTypeById(typeId).pipe(
            map(type => type?.formConfig || [])
        );
    }

    // For development/fallback use
    private getStaticProductTypes(): ProductType[] {
        type AttributeType = ProductTypeAttribute['type'];

        return [
            {
                id: 'clothing',
                name: 'Clothing',
                description: 'Apparel items including shirts, pants, dresses, etc.',
                icon: 'checkroom',
                attributes: [
                    {
                        id: 'size',
                        name: 'Sizes',
                        type: 'multiselect' as AttributeType,
                        isRequired: true,
                        options: [
                            { label: 'XS', value: 'XS' },
                            { label: 'S', value: 'S' },
                            { label: 'M', value: 'M' },
                            { label: 'L', value: 'L' },
                            { label: 'XL', value: 'XL' },
                            { label: 'XXL', value: 'XXL' }
                        ],
                        displayOrder: 1,
                        isFilterable: true,
                        isComparable: false
                    },
                    {
                        id: 'color',
                        name: 'Colors',
                        type: 'color' as AttributeType,
                        isRequired: true,
                        displayOrder: 2,
                        isFilterable: true,
                        isComparable: false
                    },
                    {
                        id: 'material',
                        name: 'Material',
                        type: 'text' as AttributeType,
                        isRequired: false,
                        displayOrder: 3,
                        isFilterable: true,
                        isComparable: true
                    },
                    {
                        id: 'gender',
                        name: 'Gender',
                        type: 'select' as AttributeType,
                        isRequired: true,
                        options: [
                            { label: 'Men', value: 'men' },
                            { label: 'Women', value: 'women' },
                            { label: 'Unisex', value: 'unisex' },
                            { label: 'Boys', value: 'boys' },
                            { label: 'Girls', value: 'girls' }
                        ],
                        displayOrder: 4,
                        isFilterable: true,
                        isComparable: false
                    }
                ],
                formConfig: [] // Will be generated dynamically based on attributes
            },
            {
                id: 'electronics',
                name: 'Electronics',
                description: 'Electronic devices and accessories',
                icon: 'devices',
                attributes: [
                    {
                        id: 'brand',
                        name: 'Brand',
                        type: 'text' as AttributeType,
                        isRequired: true,
                        displayOrder: 1,
                        isFilterable: true,
                        isComparable: true
                    },
                    {
                        id: 'model',
                        name: 'Model',
                        type: 'text' as AttributeType,
                        isRequired: true,
                        displayOrder: 2,
                        isFilterable: true,
                        isComparable: true
                    },
                    {
                        id: 'warranty',
                        name: 'Warranty Period (months)',
                        type: 'number' as AttributeType,
                        isRequired: false,
                        displayOrder: 3,
                        isFilterable: true,
                        isComparable: true
                    }
                ],
                formConfig: [] // Will be generated dynamically based on attributes
            },
            {
                id: 'books',
                name: 'Books',
                description: 'Books, publications, and literature',
                icon: 'menu_book',
                attributes: [
                    {
                        id: 'author',
                        name: 'Author',
                        type: 'text' as AttributeType,
                        isRequired: true,
                        displayOrder: 1,
                        isFilterable: true,
                        isComparable: false
                    },
                    {
                        id: 'isbn',
                        name: 'ISBN',
                        type: 'text' as AttributeType,
                        isRequired: true,
                        displayOrder: 2,
                        isFilterable: false,
                        isComparable: false,
                        validation: {
                            pattern: '^(?:ISBN(?:-1[03])?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[- ]){4})[- 0-9]{17}$)(?:97[89][- ]?)?[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$',
                            message: 'Please enter a valid ISBN'
                        }
                    },
                    {
                        id: 'pages',
                        name: 'Page Count',
                        type: 'number' as AttributeType,
                        isRequired: false,
                        displayOrder: 3,
                        isFilterable: false,
                        isComparable: true
                    }
                ],
                formConfig: [] // Will be generated dynamically based on attributes
            }
        ].map(type => ({
            ...type,
            formConfig: this.generateFormConfigFromAttributes(type.attributes)
        }));
    }

    // Dynamically converts ProductTypeAttributes to FormlyFieldConfig objects
    private generateFormConfigFromAttributes(attributes: any[]): FormlyFieldConfig[] {
        return attributes.map(attr => {
            const baseField: FormlyFieldConfig = {
                key: attr.id,
                props: {
                    label: attr.name,
                    description: attr.description,
                    required: attr.isRequired,
                    options: attr.options
                }
            };

            // Set field type based on attribute type
            switch (attr.type) {
                case 'text':
                    baseField.type = 'input';
                    break;
                case 'number':
                    baseField.type = 'input';
                    baseField.props = {
                        ...baseField.props,
                        type: 'number',
                        min: attr.validation?.min,
                        max: attr.validation?.max
                    };
                    break;
                case 'boolean':
                    baseField.type = 'checkbox';
                    break;
                case 'select':
                    baseField.type = 'select';
                    break;
                case 'multiselect':
                    baseField.type = 'select';
                    baseField.props = {
                        ...baseField.props,
                        multiple: true
                    };
                    break;
                case 'color':
                    baseField.type = 'color-picker'; // Custom field type
                    break;
                case 'size':
                    baseField.type = 'size-matrix'; // Custom field type
                    break;
                case 'dimension':
                    baseField.type = 'dimension'; // Custom field type
                    break;
                default:
                    baseField.type = 'input';
            }

            // Add validation if provided
            if (attr.validation) {
                baseField.validation = {
                    messages: {}
                };

                if (attr.validation.pattern) {
                    baseField.validators = {
                        pattern: {
                            expression: (c: any) => !c.value || new RegExp(attr.validation.pattern).test(c.value),
                            message: attr.validation.message || `${attr.name} is not in a valid format`
                        }
                    };
                }
            }

            return baseField;
        });
    }
}