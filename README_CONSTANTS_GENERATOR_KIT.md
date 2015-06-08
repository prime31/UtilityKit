ConstantsGeneratorKit
=====================

This little script will generate static classes for layers, scenes, tags and resources. The script will rebuild the constant classes anytime it detects a change (via OnPostprocessAllAssets). Additionaly, you can manually trigger a regeneration via the "Edit/Generate Constants Classes" menu item. It will spit out the constants classes in the scripts/auto-generated folder by default in the "k" namespace. This allows you can use autocomplete to easily access them without having to worry about future changes and random ints/strings all over your code.

Note that generation by default will only happen if you use the Edit -> Generate Constants Classes menu item. You can turn on auto generation by opening the ConstantsGeneratorKit.cs file and uncommenting the first line defining the DISABLE_AUTO_GENERATION symbol.

You can configure where the classes will be written and which namespace they will be in by opening the ConstantsGeneratorKit.cs file and modifying FOLDER_LOCATION and NAMESPACE. You can also configure which Resources folders will be ignored by adding any folders to IGNORE_RESOURCES_IN_SUBFOLDERS. This is handy for dealing with 3rd party code or editor tools that you do not want to be included in your Resources classes though it is highly recommended that you remove any Resources folders that should not be in your final build to keep your build size down!



License
---

ConstantsGeneratorKit is just a modified, updated and expanded version of [Devin Reimer's](https://twitter.com/DevinReimer) TagsLayersScenesBuilder available on his blog [here](http://blog.almostlogical.com/resources/TagsLayersScenesBuilder.cs). Below is the license directly copy/pasted from the original TagsLayersScenesBuilder file:


>Tags, Layers and Scene Builder - Auto Generate Tags, Layers and Scenes classes containing consts for all variables for code completion - 2012-10-01  
>released under MIT License  
>http://www.opensource.org/licenses/mit-license.php  
>  
>@author		Devin Reimer - AlmostLogical Software / Owlchemy Labs  
>@website 		http://blog.almostlogical.com, http://owlchemylabs.com  


>Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
