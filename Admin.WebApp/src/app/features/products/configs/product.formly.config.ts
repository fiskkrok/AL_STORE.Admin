// src/app/features/products/configs/product.formly.config.ts
import { FormlyFieldConfig } from '@ngx-formly/core';
import { FormlyImageUploadTypeComponent } from '../../../shared/formly/image-upload.type';

export function getProductFormFields(categories: { id: string; name: string; }[]): FormlyFieldConfig[] {
    return [
        {
            fieldGroupClassName: 'grid grid-cols-1 md:grid-cols-2 gap-4',
            fieldGroup: [
                {
                    className: 'col-span-1 md:col-span-2',
                    fieldGroup: [
                        {
                            key: 'basicInfo',
                            templateOptions: { label: 'Basic Information' },
                            fieldGroup: [
                                {
                                    key: 'name',
                                    type: 'input',
                                    templateOptions: {
                                        label: 'Product Name',
                                        placeholder: 'Enter product name',
                                        required: true,
                                        minLength: 3,
                                        maxLength: 100
                                    },
                                    validation: {
                                        messages: {
                                            required: 'Product name is required',
                                            minlength: 'Name must be at least 3 characters',
                                            maxlength: 'Name cannot be more than 100 characters'
                                        }
                                    }
                                },
                                {
                                    key: 'category',
                                    type: 'select',
                                    templateOptions: {
                                        label: 'Category',
                                        placeholder: 'Select a category',
                                        required: true,
                                        options: categories.map(cat => ({
                                            label: cat.name,
                                            value: cat.id
                                        }))
                                    },
                                    validation: {
                                        messages: {
                                            required: 'Category is required'
                                        }
                                    }
                                },
                                {
                                    key: 'description',
                                    type: 'textarea',
                                    templateOptions: {
                                        label: 'Description',
                                        placeholder: 'Enter product description',
                                        required: true,
                                        rows: 4,
                                        maxLength: 1000
                                    },
                                    validation: {
                                        messages: {
                                            required: 'Description is required',
                                            maxlength: 'Description cannot be more than 1000 characters'
                                        }
                                    }
                                }
                            ]
                        }
                    ]
                },
                {
                    className: 'col-span-1',
                    fieldGroup: [
                        {
                            key: 'pricing',
                            templateOptions: { label: 'Pricing & Stock' },
                            fieldGroup: [
                                {
                                    key: 'price',
                                    type: 'input',
                                    templateOptions: {
                                        type: 'number',
                                        label: 'Price',
                                        placeholder: 'Enter price',
                                        required: true,
                                        min: 0,
                                        addonLeft: {
                                            text: '$'
                                        }
                                    },
                                    validation: {
                                        messages: {
                                            required: 'Price is required',
                                            min: 'Price must be greater than or equal to 0'
                                        }
                                    }
                                },
                                {
                                    key: 'stock',
                                    type: 'input',
                                    templateOptions: {
                                        type: 'number',
                                        label: 'Initial Stock',
                                        placeholder: 'Enter initial stock',
                                        required: true,
                                        min: 0
                                    },
                                    validation: {
                                        messages: {
                                            required: 'Initial stock is required',
                                            min: 'Stock must be greater than or equal to 0'
                                        }
                                    }
                                }
                            ]
                        }
                    ]
                },
                {
                    className: 'col-span-1',
                    fieldGroup: [
                        {
                            key: 'images',
                            type: FormlyImageUploadTypeComponent,
                            templateOptions: {
                                label: 'Product Images',
                                multiple: true,
                                accept: 'image/*',
                                maxSize: 5000000, // 5MB
                                maxFiles: 5,
                                required: true
                            },
                            validation: {
                                messages: {
                                    required: 'At least one image is required',
                                    maxSize: 'Image size cannot exceed 5MB',
                                    maxFiles: 'Cannot upload more than 5 images'
                                }
                            }
                        }
                    ]
                }
            ]
        }
    ];
}