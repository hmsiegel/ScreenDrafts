<#
.SYNOPSIS
  Checks which of the 96 drafters have wiki pages and fetches their image URLs
  via the MediaWiki API (no Cloudflare issues).

  Run this first to see how many pages exist before attempting downloads.
#>
param(
  [string]$OutputCsv = ".\wiki-check-$(Get-Date -Format 'yyyyMMdd-HHmmss').csv"
)

$drafters = @(
  @{ Id="pe_K7s8a8c7pj00U0s"; Name="Alex Terapane"          },
  @{ Id="pe_gkslyUJZHmrhESN"; Name="Alfonso Carrillo"       },
  @{ Id="pe_4UQ4JBwne6ih6yg"; Name="Alison Leiby"           },
  @{ Id="pe_clLJd39wiziUesl"; Name="Andres Cabrera"         },
  @{ Id="pe_3g518nm9uars75l"; Name="Andrew Furtado"         },
  @{ Id="pe_n3q42cXvmgipsJ3"; Name="Ashley Coffin"          },
  @{ Id="pe_OMDxVcAoO8tE9ji"; Name="Ben Hethcoat"           },
  @{ Id="pe_GkOW7yKhIDCBmbS"; Name="Ben Mankiewicz"         },
  @{ Id="pe_rZqNmp0BXnocj3s"; Name="Ben Mekler"             },
  @{ Id="pe_iD1VEAn0g2xFHo8"; Name="Beth Crudele"           },
  @{ Id="pe_XAAnfVVAzn3Xh5G"; Name="Bilge Ebiri"            },
  @{ Id="pe_OsM4alvNQjhjwXb"; Name="Bill Bria"              },
  @{ Id="pe_SEX4gf706aWcHXQ"; Name="B.J. Colangelo"         },
  @{ Id="pe_L9JlHh6aRcNDnnf"; Name="Brandon Streussnig"     },
  @{ Id="pe_hXAz63cC89xK8E9"; Name="Breanna Whipple"        },
  @{ Id="pe_Pi7XYBFtbIaWYTY"; Name="Britt Keller"           },
  @{ Id="pe_Bs1i5PbbPW2iQyL"; Name="Chris Amick"            },
  @{ Id="pe_yQNxRwUu8BK91CB"; Name="Chris Owens"            },
  @{ Id="pe_e4D7QVRSYsa36EP"; Name="Chris Thomas Devlin"    },
  @{ Id="pe_SbVgWefkwsBNDqK"; Name="Christian Holub"        },
  @{ Id="pe_2P3WxTpEkFeIUg5"; Name="Clem Bastow"            },
  @{ Id="pe_QTLf1msKeTP1psI"; Name="Cody Downs"             },
  @{ Id="pe_scq0RagLSN4bgKA"; Name="Cory Everett"           },
  @{ Id="pe_IquVMdxAwJBjqe0"; Name="Cyntisha Coats"         },
  @{ Id="pe_ZWkOsQFejUUyezw"; Name="Dane McDonald"          },
  @{ Id="pe_DHr0smyPFKymCti"; Name="Daniel Crooke"          },
  @{ Id="pe_9sBj9Jqo8sdyuys"; Name="Dave Gonzales"          },
  @{ Id="pe_eERxl99GNU2Frkk"; Name="Dave Holmes"            },
  @{ Id="pe_4dr6cWoefI0MCvR"; Name="Dave White"             },
  @{ Id="pe_IF14ilRaXPC86jC"; Name="David Chen"             },
  @{ Id="pe_ETbNBIyGJDtv42a"; Name="Demma Strausbaugh"      },
  @{ Id="pe_V24VVG9DVmF0HGd"; Name="Derek Lawrence"         },
  @{ Id="pe_KwmI2zEuy6bF88l"; Name="Devan Coggan"           },
  @{ Id="pe_cyByMx90lFPLlum"; Name="Dory Benford"           },
  @{ Id="pe_EFORDSZgCUkgP5I"; Name="Doug Jones"             },
  @{ Id="pe_WioVtpqAN87effg"; Name="Dylan Guerra"            },
  @{ Id="pe_UV3ll668hKvLoeX"; Name="Elric Kane"             },
  @{ Id="pe_EhAjhrVPTZoGyCC"; Name="Emily Hagins"           },
  @{ Id="pe_pxuzzYKm5543vpI"; Name="Eric Darling"           },
  @{ Id="pe_zVz8WpcSgTb5tQU"; Name="Eric Plese"             },
  @{ Id="pe_JZOKHq63pr58OQo"; Name="Esther Zuckerman"       },
  @{ Id="pe_9eIiyoIDoLIG9vf"; Name="Frank Berman"           },
  @{ Id="pe_hiRo70YxgEBm1Lj"; Name="Heidi Honeycutt"        },
  @{ Id="pe_638H54KtoRATqRF"; Name="Imani Davis"            },
  @{ Id="pe_nu1rdnThiZxXzi2"; Name="Jason Revaldt"          },
  @{ Id="pe_CBT2sluHmqM7NdM"; Name="Jason Sheridan"         },
  @{ Id="pe_KaiVs5sWhjOh5si"; Name="Jen Johans"             },
  @{ Id="pe_tu1b42U8oIKZ7eT"; Name="Jill Gevargizian"       },
  @{ Id="pe_nldFTrQDHjh1j4M"; Name="Jocey Coffman"          },
  @{ Id="pe_jXcOFtDgi8e94ZT"; Name="Joe Reid"               },
  @{ Id="pe_Tj8LObqpubYmby1"; Name="John Freiler"           },
  @{ Id="pe_egTq2QoGUZL5pH0"; Name="Jonathan Moler"         },
  @{ Id="pe_Z23meOnAiQclEoL"; Name="Jordan Hoffman"         },
  @{ Id="pe_Cmk0o42ZpjOjCzJ"; Name="Juan Quanito"           },
  @{ Id="pe_jzhq07fVLKQ6XCc"; Name="Julia Marchese"         },
  @{ Id="pe_6tS8W7Y7wZCgEzL"; Name="Justin LaLiberty"       },
  @{ Id="pe_PTfjuzv7EDw503d"; Name="Katey Rich"             },
  @{ Id="pe_k19K8hDInQTnzhk"; Name="Katie Walsh"            },
  @{ Id="pe_905uBXTLS0qOcP0"; Name="Kenny Neibart"          },
  @{ Id="pe_ts3Us1ZqaKzMURG"; Name="Kevin Costello"         },
  @{ Id="pe_YbRATzcBT43tNhL"; Name="Kristy Puchko"          },
  @{ Id="pe_yhHn6nZETfsXSeV"; Name="Kyle Turner"            },
  @{ Id="pe_GaBWRmdixaM2DqC"; Name="Larry Fessenden"        },
  @{ Id="pe_3YMvRGAP9Wso4cc"; Name="Linda Holmes"           },
  @{ Id="pe_ucPcTyoUice3vij"; Name="Liz Shannon Miller"     },
  @{ Id="pe_Pg2LKFZwBA0sMNG"; Name="Luis Rendon"            },
  @{ Id="pe_EizA79C6Do0FbGK"; Name="Mallory O'Meara"        },
  @{ Id="pe_YUPqNwvNT39NBiz"; Name="Marc Bernardin"         },
  @{ Id="pe_D95M1pWQxGE1FaS"; Name="Mark Rozeman"           },
  @{ Id="pe_XbV0XZvd4VSW0md"; Name="Mary Beth McAndrews"    },
  @{ Id="pe_6S06I8qBm0VvFu0"; Name="Matthew Krol"           },
  @{ Id="pe_ztpIHchs0Ha9Dn1"; Name="Matt Mercer"            },
  @{ Id="pe_zR1ugVOtJ17EsDz"; Name="Max Genecov"            },
  @{ Id="pe_62pdGDkOjeGfyg1"; Name="Meredith Smith-Hitchman"},
  @{ Id="pe_oF0j42H272xXBqG"; Name="Milla Bell-Hart"        },
  @{ Id="pe_XNhgKy2ZDMqZScW"; Name="Mitchell Beaupre"       },
  @{ Id="pe_drsnmXwlemjeiRS"; Name="Nichol Lovett"          },
  @{ Id="pe_1yW6W3U0lozczTT"; Name="Nick de Semlyen"        },
  @{ Id="pe_OuWt3Sadagp2GNH"; Name="Pat Driscoll"           },
  @{ Id="pe_mrGRu92JAYa7wgj"; Name="Patrick Bromley"        },
  @{ Id="pe_GbkmEiWeFGTfO62"; Name="Patrick Hamilton"       },
  @{ Id="pe_sZEN8JhzNm6HNVf"; Name="Patrick Willems"        },
  @{ Id="pe_1bugl8KbBXWIvd6"; Name="Penny Cox"              },
  @{ Id="pe_P5t05CV5UzPtemz"; Name="Piya Sinha-Roy"         },
  @{ Id="pe_3c0WkmljPOLvOEA"; Name="Rachel Walker"          },
  @{ Id="pe_jgZmhzJjWxCSgzk"; Name="Rance Collins"          },
  @{ Id="pe_rOfMo3JBblvF61g"; Name="Robert Butler III"      },
  @{ Id="pe_D6eCHG0QSSBM9RY"; Name="Roxana Hadadi"          },
  @{ Id="pe_BkJgF9N72Z0Eaof"; Name="Samm Deighan"           },
  @{ Id="pe_HW28zazbfgQ9CyS"; Name="Sarah Sterling"         },
  @{ Id="pe_fKduPTsTFeGeDoU"; Name="Scott Wampler"          },
  @{ Id="pe_KFtj62CZrr6ZRpw"; Name="Teri Gamble"            },
  @{ Id="pe_CrEBgROKI2uKVDh"; Name="Thomas Vitale"          },
  @{ Id="pe_FtZ2My84La3boGq"; Name="Toshi McWeeny"          },
  @{ Id="pe_voTWpASoeXPJq8s"; Name="Vince Balzano"          },
  @{ Id="pe_HxuzJ3Yamki6YN5"; Name="Walter Hollmann"        }
)

# Batch the API calls — MediaWiki supports up to 50 titles per request
$baseUrl = "https://screendrafts.fandom.com/api.php"
$results = @()
$batchSize = 50

for ($i = 0; $i -lt $drafters.Count; $i += $batchSize) {
  $batch = $drafters[$i .. [Math]::Min($i + $batchSize - 1, $drafters.Count - 1)]
  $encodedTitles = ($batch | ForEach-Object { [Uri]::EscapeDataString(($_.Name -replace ' ', '_')) }) -join '|'
  $apiUrl = $baseUrl + '?action=query' + '&titles=' + $encodedTitles + '&prop=pageimages' + '&format=json' + '&pithumbsize=500'

  Write-Host "Querying batch $([Math]::Floor($i/$batchSize) + 1)..." -ForegroundColor Cyan

  $response = Invoke-RestMethod -Uri $apiUrl -UseBasicParsing
  $pages = $response.query.pages

  # Build a lookup from normalised title → image data
  $titleMap = @{}
  foreach ($page in $pages.PSObject.Properties) {
    $p = $page.Value
    $titleMap[$p.title.ToLower()] = $p
  }

  foreach ($d in $batch) {
    $page = $titleMap[$d.Name.ToLower()]
    if (-not $page -or $page.missing -ne $null) {
      Write-Host "  ✗ $($d.Name)" -ForegroundColor Red
      $results += [PSCustomObject]@{
        PublicId   = $d.Id
        Name       = $d.Name
        HasPage    = $false
        ImageUrl   = ''
        PageImage  = ''
      }
    } else {
      $imgUrl = $page.thumbnail.source ?? ''
      # Strip the scale-to-width-down parameter to get the full-size image
      $fullUrl = $imgUrl -replace '/scale-to-width-down/\d+', ''
      Write-Host "  ✓ $($d.Name)  →  $($page.pageimage)" -ForegroundColor Green
      $results += [PSCustomObject]@{
        PublicId   = $d.Id
        Name       = $d.Name
        HasPage    = $true
        ImageUrl   = $fullUrl
        PageImage  = $page.pageimage ?? ''
      }
    }
  }

  Start-Sleep -Milliseconds 300
}

$results | Export-Csv -Path $OutputCsv -NoTypeInformation -Encoding UTF8

$found    = ($results | Where-Object { $_.HasPage }).Count
$missing  = ($results | Where-Object { -not $_.HasPage }).Count

Write-Host ""
Write-Host "─────────────────────────────────────────" -ForegroundColor Cyan
Write-Host "  Have wiki page  : $found"   -ForegroundColor Green
Write-Host "  No wiki page    : $missing" -ForegroundColor Yellow
Write-Host "  CSV             : $OutputCsv"
Write-Host "─────────────────────────────────────────" -ForegroundColor Cyan