import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'ceil',
    pure: true,
    standalone: true
})

export class CeilPipe implements PipeTransform {
    transform(value: number): number {
        return Math.ceil(value);

    }
}