﻿using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;

public record ProductImageRemovedDomainEvent(Product Product, ProductImage Image) : DomainEvent;