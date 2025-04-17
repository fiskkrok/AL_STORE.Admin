// src/app/core/services/product-type.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map, shareReplay, tap } from 'rxjs/operators';
import { FormlyFieldConfig } from '@ngx-formly/core';
import { ProductType } from '../../shared/models/product-type.model';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ProductTypeService {
    private readonly apiUrl = `${environment.apiUrls.admin.products}/types`;
    private cachedProductTypes: Observable<ProductType[]> | null = null;
    private lastCacheUpdate: number = 0; // Add this line

    constructor(private readonly http: HttpClient) { } // Add readonly here

    getProductTypes(forceRefresh: boolean = false): Observable<ProductType[]> {
        if (!this.cachedProductTypes || forceRefresh || this.cacheExpired()) {
            this.cachedProductTypes = this.http.get<ProductType[]>(this.apiUrl, {
                headers: {
                    'api-version': '1.0'
                }
            }).pipe(
                map(types => types.map(type => ({
                    ...type,
                    formConfig: this.generateFormConfigFromAttributes(type.attributes)
                }))),
                tap(() => {
                    this.lastCacheUpdate = Date.now(); // This line will now work
                }),
                catchError(error => {
                    console.error('Error fetching product types:', error);
                    return of(error);
                }),
                shareReplay(1)
            );
        }
        return this.cachedProductTypes;
    }

    private cacheExpired(): boolean {
        const now = Date.now();
        const cacheLifetime = environment.cache.productTypesTTL || 3600000; // 1 hour default
        // Check if lastCacheUpdate is 0 (never updated) or if the cache lifetime has passed
        return this.lastCacheUpdate === 0 || (this.lastCacheUpdate + cacheLifetime < now); // This line will now work
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
                },
                defaultValue: attr.isRequired ? this.getDefaultValueForType(attr.type, attr.defaultValue) : undefined
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

    private getDefaultValueForType(type: string, providedDefault: any): any {
        if (providedDefault !== undefined) return providedDefault;

        switch (type) {
            case 'text': return '';
            case 'number': return 0;
            case 'boolean': return false;
            case 'select': return '';
            case 'multiselect': return [];
            case 'color': return '#000000';
            case 'size': return {};
            case 'dimension': return {};
            default: return '';
        }
    }
}