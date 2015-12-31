# TSmach
=*_TSmstch_* description= 
_*TSmatch*_ system seek and connect with the model in Tekla Structures the matching materials in the information data base.
Created in 2016 based on _*match*_ VBA and C# code developed by Alexander Pass and Pavel Khrapkin earlier in 2010-2013.
Open source project *_match_* is available on https://code.google.com/p/match/ -- now transfered to GitHub as github.com/PavelKhrapkin/match.

==General idea:==
TSmach has access to the model in Tekla over OpenAPI. After read a model attributes, TSmatch watch the material files, collected in his _information repository_. This repository is set of Excel files, now in *_TSmatch_* extended with Internet content.
As a result of work *_TSmatch_* generate *Report* file, which contains list of found in the information repository entries. This Report could be Updated, when the model is changed. 
Information repository collection also could be updated with the new source files.
