using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Common.Models;
public class ProductAttributeRequest
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Type { get; set; }
}
