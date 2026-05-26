<#
.SYNOPSIS
  Scrapes profile pictures from the ScreenDrafts Fandom wiki for the 96 drafters
  whose avatars are missing from the local folder, saves them as {publicId}.{ext},
  and updates drafts.people.profile_picture_path in PostgreSQL.

.PARAMETER DryRun
  Logs planned actions without downloading files or updating the DB.

.PARAMETER AvatarDir
  Destination folder for downloaded images.

.PARAMETER PgHost / PgPort / PgDatabase / PgUser / PgPassword
  PostgreSQL connection parameters.

.EXAMPLE
  .\Scrape-DrafterAvatars.ps1 -DryRun
  .\Scrape-DrafterAvatars.ps1
#>
param(
  [switch]$DryRun,
  [string]$AvatarDir  = "C:\Repos\ScreenDrafts\src\screendrafts.ui\public\drafters",
  [string]$PgHost     = "localhost",
  [string]$PgPort     = "5432",
  [string]$PgDatabase = "screendrafts",
  [string]$PgUser     = "postgres",
  [string]$PgPassword = "postgres"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Locate psql.exe ───────────────────────────────────────────────────────────

$psql = Get-Command psql -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty Source

if (-not $psql) {
  $candidates = @(
    "C:\Program Files\PostgreSQL\18\bin\psql.exe",
    "C:\Program Files\PostgreSQL\17\bin\psql.exe",
    "C:\Program Files\PostgreSQL\16\bin\psql.exe",
    "C:\Program Files\PostgreSQL\15\bin\psql.exe"
  )
  $psql = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if (-not $psql) {
  throw "psql.exe not found. Add PostgreSQL bin to PATH or install PostgreSQL client tools."
}

$env:PGPASSWORD = $PgPassword
$psqlArgs = @("-h", $PgHost, "-p", $PgPort, "-U", $PgUser, "-d", $PgDatabase, "-t", "-A")

function Invoke-Psql([string]$sql) {
  $output = & $psql @psqlArgs -c $sql 2>&1
  if ($LASTEXITCODE -ne 0) { throw "psql error: $output" }
  return $output
}

# ── Drafter list (public_id | display_name | first_name | last_name) ──────────
# Sourced from the query: SELECT public_id, display_name, first_name, last_name
# FROM drafts.people WHERE profile_picture_path NOT LIKE public_id || '.%'

$drafters = @(
  @{ Id="pe_K7s8a8c7pj00U0s"; First="Alex";        Last="Trepane"         },
  @{ Id="pe_gkslyUJZHmrhESN"; First="Alfonso";      Last="Carrillo"        },
  @{ Id="pe_4UQ4JBwne6ih6yg"; First="Alison";       Last="Leiby"           },
  @{ Id="pe_clLJd39wiziUesl"; First="Andres";       Last="Cabrera"         },
  @{ Id="pe_3g518nm9uars75l"; First="Andrew";       Last="Furtado"         },
  @{ Id="pe_n3q42cXvmgipsJ3"; First="Ashley";       Last="Coffin"          },
  @{ Id="pe_OMDxVcAoO8tE9ji"; First="Ben";          Last="Hethcoat"        },
  @{ Id="pe_GkOW7yKhIDCBmbS"; First="Ben";          Last="Mankiewicz"      },
  @{ Id="pe_rZqNmp0BXnocj3s"; First="Ben";          Last="Mekler"          },
  @{ Id="pe_iD1VEAn0g2xFHo8"; First="Beth";         Last="Crudele"         },
  @{ Id="pe_XAAnfVVAzn3Xh5G"; First="Bilge";        Last="Ebiri"           },
  @{ Id="pe_OsM4alvNQjhjwXb"; First="Bill";         Last="Bria"            },
  @{ Id="pe_SEX4gf706aWcHXQ"; First="B.J.";         Last="Colangelo"       },
  @{ Id="pe_L9JlHh6aRcNDnnf"; First="Brandon";      Last="Streussnig"      },
  @{ Id="pe_hXAz63cC89xK8E9"; First="Breanna";      Last="Whipple"         },
  @{ Id="pe_Pi7XYBFtbIaWYTY"; First="Britt";        Last="Keller"          },
  @{ Id="pe_Bs1i5PbbPW2iQyL"; First="Chris";        Last="Amick"           },
  @{ Id="pe_yQNxRwUu8BK91CB"; First="Chris";        Last="Owens"           },
  @{ Id="pe_e4D7QVRSYsa36EP"; First="Chris Thomas"; Last="Devlin"          },
  @{ Id="pe_SbVgWefkwsBNDqK"; First="Christian";    Last="Holub"           },
  @{ Id="pe_2P3WxTpEkFeIUg5"; First="Clem";         Last="Bastow"          },
  @{ Id="pe_QTLf1msKeTP1psI"; First="Cody";         Last="Downs"           },
  @{ Id="pe_scq0RagLSN4bgKA"; First="Cory";         Last="Everett"         },
  @{ Id="pe_IquVMdxAwJBjqe0"; First="Cyntisha";     Last="Coats"           },
  @{ Id="pe_ZWkOsQFejUUyezw"; First="Dane";         Last="McDonald"        },
  @{ Id="pe_DHr0smyPFKymCti"; First="Daniel";       Last="Crooke"          },
  @{ Id="pe_9sBj9Jqo8sdyuys"; First="Dave";         Last="Gonzales"        },
  @{ Id="pe_eERxl99GNU2Frkk"; First="Dave";         Last="Holmes"          },
  @{ Id="pe_4dr6cWoefI0MCvR"; First="Dave";         Last="White"           },
  @{ Id="pe_IF14ilRaXPC86jC"; First="David";        Last="Chen"            },
  @{ Id="pe_ETbNBIyGJDtv42a"; First="Demma";        Last="Strausbaugh"     },
  @{ Id="pe_V24VVG9DVmF0HGd"; First="Derek";        Last="Lawrence"        },
  @{ Id="pe_KwmI2zEuy6bF88l"; First="Devan";        Last="Coggan"          },
  @{ Id="pe_cyByMx90lFPLlum"; First="Dory";         Last="Benford"         },
  @{ Id="pe_EFORDSZgCUkgP5I"; First="Doug";         Last="Jones"           },
  @{ Id="pe_WioVtpqAN87effg"; First="Dylan";        Last="Guerra"          },
  @{ Id="pe_UV3ll668hKvLoeX"; First="Elric";        Last="Kane"            },
  @{ Id="pe_EhAjhrVPTZoGyCC"; First="Emily";        Last="Hagins"          },
  @{ Id="pe_pxuzzYKm5543vpI"; First="Eric";         Last="Darling"         },
  @{ Id="pe_zVz8WpcSgTb5tQU"; First="Eric";         Last="Plese"           },
  @{ Id="pe_JZOKHq63pr58OQo"; First="Esther";       Last="Zuckerman"       },
  @{ Id="pe_9eIiyoIDoLIG9vf"; First="Frank";        Last="Berman"          },
  @{ Id="pe_hiRo70YxgEBm1Lj"; First="Heidi";        Last="Honeycutt"       },
  @{ Id="pe_638H54KtoRATqRF"; First="Imani";        Last="Davis"           },
  @{ Id="pe_nu1rdnThiZxXzi2"; First="Jason";        Last="Revaldt"         },
  @{ Id="pe_CBT2sluHmqM7NdM"; First="Jason";        Last="Sheridan"        },
  @{ Id="pe_KaiVs5sWhjOh5si"; First="Jen";          Last="Johans"          },
  @{ Id="pe_tu1b42U8oIKZ7eT"; First="Jill";         Last="Gevargizian"     },
  @{ Id="pe_nldFTrQDHjh1j4M"; First="Jocey";        Last="Coffman"         },
  @{ Id="pe_jXcOFtDgi8e94ZT"; First="Joe";          Last="Reid"            },
  @{ Id="pe_Tj8LObqpubYmby1"; First="John";         Last="Freiler"         },
  @{ Id="pe_egTq2QoGUZL5pH0"; First="Jonathan";     Last="Moler"           },
  @{ Id="pe_Z23meOnAiQclEoL"; First="Jordan";       Last="Hoffman"         },
  @{ Id="pe_Cmk0o42ZpjOjCzJ"; First="Juan";         Last="Quanito"         },
  @{ Id="pe_jzhq07fVLKQ6XCc"; First="Julia";        Last="Marchese"        },
  @{ Id="pe_6tS8W7Y7wZCgEzL"; First="Justin";       Last="LaLiberty"       },
  @{ Id="pe_PTfjuzv7EDw503d"; First="Katey";        Last="Rich"            },
  @{ Id="pe_k19K8hDInQTnzhk"; First="Katie";        Last="Walsh"           },
  @{ Id="pe_905uBXTLS0qOcP0"; First="Kenny";        Last="Neibart"         },
  @{ Id="pe_ts3Us1ZqaKzMURG"; First="Kevin";        Last="Costello"        },
  @{ Id="pe_YbRATzcBT43tNhL"; First="Kristy";       Last="Puchko"          },
  @{ Id="pe_yhHn6nZETfsXSeV"; First="Kyle";         Last="Turner"          },
  @{ Id="pe_GaBWRmdixaM2DqC"; First="Larry";        Last="Fessenden"       },
  @{ Id="pe_3YMvRGAP9Wso4cc"; First="Linda";        Last="Holmes"          },
  @{ Id="pe_ucPcTyoUice3vij"; First="Liz Shannon";  Last="Miller"          },
  @{ Id="pe_Pg2LKFZwBA0sMNG"; First="Luis";         Last="Rendon"          },
  @{ Id="pe_EizA79C6Do0FbGK"; First="Mallory";      Last="O'Meara"         },
  @{ Id="pe_YUPqNwvNT39NBiz"; First="Marc";         Last="Bernardin"       },
  @{ Id="pe_D95M1pWQxGE1FaS"; First="Mark";         Last="Rozeman"         },
  @{ Id="pe_XbV0XZvd4VSW0md"; First="Mary Beth";    Last="McAndrews"       },
  @{ Id="pe_6S06I8qBm0VvFu0"; First="Matthew";      Last="Krol"            },
  @{ Id="pe_ztpIHchs0Ha9Dn1"; First="Matt";         Last="Mercer"          },
  @{ Id="pe_zR1ugVOtJ17EsDz"; First="Max";          Last="Genecov"         },
  @{ Id="pe_62pdGDkOjeGfyg1"; First="Meredith";     Last="Smith-Hitchman"  },
  @{ Id="pe_oF0j42H272xXBqG"; First="Milla";        Last="Bell-Hart"       },
  @{ Id="pe_XNhgKy2ZDMqZScW"; First="Mitchell";     Last="Beaupre"         },
  @{ Id="pe_drsnmXwlemjeiRS"; First="Nichol";       Last="Lovett"          },
  @{ Id="pe_1yW6W3U0lozczTT"; First="Nick de";      Last="Semlyen"         },
  @{ Id="pe_OuWt3Sadagp2GNH"; First="Pat";          Last="Driscoll"        },
  @{ Id="pe_mrGRu92JAYa7wgj"; First="Patrick";      Last="Bromley"         },
  @{ Id="pe_GbkmEiWeFGTfO62"; First="Patrick";      Last="Hamilton"        },
  @{ Id="pe_sZEN8JhzNm6HNVf"; First="Patrick";      Last="Willems"         },
  @{ Id="pe_1bugl8KbBXWIvd6"; First="Penny";        Last="Cox"             },
  @{ Id="pe_P5t05CV5UzPtemz"; First="Piya";         Last="Sinha-Roy"       },
  @{ Id="pe_3c0WkmljPOLvOEA"; First="Rachel";       Last="Walker"          },
  @{ Id="pe_jgZmhzJjWxCSgzk"; First="Rance";        Last="Collins"         },
  @{ Id="pe_rOfMo3JBblvF61g"; First="Robert";       Last="Butler III"      },
  @{ Id="pe_D6eCHG0QSSBM9RY"; First="Roxana";       Last="Hadadi"          },
  @{ Id="pe_BkJgF9N72Z0Eaof"; First="Samm";         Last="Deighan"         },
  @{ Id="pe_HW28zazbfgQ9CyS"; First="Sarah";        Last="Sterling"        },
  @{ Id="pe_fKduPTsTFeGeDoU"; First="Scott";        Last="Wampler"         },
  @{ Id="pe_KFtj62CZrr6ZRpw"; First="Teri";         Last="Gamble"          },
  @{ Id="pe_CrEBgROKI2uKVDh"; First="Thomas";       Last="Vitale"          },
  @{ Id="pe_FtZ2My84La3boGq"; First="Toshi";        Last="McWeeny"         },
  @{ Id="pe_voTWpASoeXPJq8s"; First="Vince";        Last="Balzano"         },
  @{ Id="pe_HxuzJ3Yamki6YN5"; First="Walter";       Last="Hollmann"        }
)

# ── Helper: build wiki page URL ───────────────────────────────────────────────

function Get-WikiUrl([string]$first, [string]$last) {
  # "Chris Thomas Devlin" → "Chris_Thomas_Devlin"
  # "B.J. Colangelo"      → "B.J._Colangelo"
  $full = "$first $last".Trim() -replace '\s+', '_'
  return "https://screendrafts.fandom.com/wiki/$full"
}

# ── Helper: scrape image URL from wiki page ───────────────────────────────────

function Get-WikiImageUrl([string]$pageUrl) {
  try {
    $response = Invoke-WebRequest -Uri $pageUrl -UseBasicParsing -TimeoutSec 15
    # The profile image is inside .pi-image-thumbnail or the first <img> in
    # .pi-item.pi-image — fall back to any wikia image URL containing the name.
    $html = $response.Content

    # Strategy 1: portable infobox image src
    if ($html -match 'class="pi-image-thumbnail"[^>]*src="([^"]+)"') {
      return $Matches[1]
    }

    # Strategy 2: any static.wikia.nocookie.net image that isn't a icon/logo
    $imgMatches = [regex]::Matches($html, 'src="(https://static\.wikia\.nocookie\.net/screendrafts/images/[^"]+)"')
    foreach ($m in $imgMatches) {
      $src = $m.Groups[1].Value
      # Skip tiny thumbnails (scale/width params indicate a resized version)
      if ($src -notmatch '/scale-to-width-down/\d+' -or $src -match 'revision/latest\?') {
        return $src
      }
    }

    # Strategy 3: first wikia image of any size
    if ($imgMatches.Count -gt 0) {
      return $imgMatches[0].Groups[1].Value
    }

    return $null
  } catch {
    return $null
  }
}

# ── Helper: determine extension from URL or Content-Type ─────────────────────

function Get-ImageExt([string]$url, [string]$contentType) {
  if ($url -match '\.(jpg|jpeg|png|webp)') { return $Matches[1].ToLower().Replace('jpeg','jpg') }
  if ($contentType -match 'jpeg')  { return 'jpg' }
  if ($contentType -match 'png')   { return 'png' }
  if ($contentType -match 'webp')  { return 'webp' }
  return 'jpg'
}

# ── Process ───────────────────────────────────────────────────────────────────

[System.IO.Directory]::CreateDirectory($AvatarDir) | Out-Null  # no-op if exists

$results   = @()
$downloaded = 0
$notFound   = 0
$errors     = 0

foreach ($d in $drafters) {
  $wikiUrl = Get-WikiUrl $d.First $d.Last
  Write-Host "  $($d.First) $($d.Last)" -NoNewline

  if ($DryRun) {
    Write-Host "  → [DRY RUN] $wikiUrl" -ForegroundColor Yellow
    $results += [PSCustomObject]@{
      Name     = "$($d.First) $($d.Last)"
      PublicId = $d.Id
      WikiUrl  = $wikiUrl
      ImageUrl = ''
      SavedAs  = ''
      Status   = 'DRY_RUN'
      Error    = ''
    }
    continue
  }

  $imageUrl = Get-WikiImageUrl $wikiUrl

  if (-not $imageUrl) {
    Write-Host "  ✗ no image found" -ForegroundColor Red
    $results += [PSCustomObject]@{
      Name     = "$($d.First) $($d.Last)"
      PublicId = $d.Id
      WikiUrl  = $wikiUrl
      ImageUrl = ''
      SavedAs  = ''
      Status   = 'NOT_FOUND'
      Error    = ''
    }
    $notFound++
    continue
  }

  try {
    $imgResponse = Invoke-WebRequest -Uri $imageUrl -UseBasicParsing -TimeoutSec 20
    $ct  = $imgResponse.Headers['Content-Type'] ?? ''
    $ext = Get-ImageExt $imageUrl $ct
    $fileName    = "$($d.Id).$ext"
    $destPath    = Join-Path $AvatarDir $fileName

    [System.IO.File]::WriteAllBytes($destPath, $imgResponse.Content)

    # Update DB
    $escaped  = $fileName.Replace("'", "''")
    $pubIdEsc = $d.Id.Replace("'", "''")
    Invoke-Psql "UPDATE drafts.people SET profile_picture_path = '$escaped' WHERE public_id = '$pubIdEsc';" | Out-Null

    Write-Host "  ✓ $fileName" -ForegroundColor Green
    $downloaded++

    $results += [PSCustomObject]@{
      Name     = "$($d.First) $($d.Last)"
      PublicId = $d.Id
      WikiUrl  = $wikiUrl
      ImageUrl = $imageUrl
      SavedAs  = $fileName
      Status   = 'DOWNLOADED'
      Error    = ''
    }

    # Be polite to the wiki — don't hammer it
    Start-Sleep -Milliseconds 500
  } catch {
    $err = $_.Exception.Message
    Write-Host "  ✗ $err" -ForegroundColor Red
    $results += [PSCustomObject]@{
      Name     = "$($d.First) $($d.Last)"
      PublicId = $d.Id
      WikiUrl  = $wikiUrl
      ImageUrl = $imageUrl
      SavedAs  = ''
      Status   = 'ERROR'
      Error    = $err
    }
    $errors++
  }
}

# ── Summary ───────────────────────────────────────────────────────────────────

$logPath = Join-Path $PSScriptRoot "avatar-scrape-$(Get-Date -Format 'yyyyMMdd-HHmmss').csv"
$results | Export-Csv -Path $logPath -NoTypeInformation -Encoding UTF8

Write-Host ""
Write-Host "─────────────────────────────────────────" -ForegroundColor Cyan
if ($DryRun) { Write-Host "  DRY RUN — no files downloaded" -ForegroundColor Yellow }
Write-Host "  Downloaded   : $downloaded" -ForegroundColor Green
Write-Host "  Not found    : $notFound"   -ForegroundColor $(if ($notFound -gt 0)  { 'Yellow' } else { 'Green' })
Write-Host "  Errors       : $errors"     -ForegroundColor $(if ($errors -gt 0)    { 'Red'    } else { 'Green' })
Write-Host "  Log          : $logPath"
Write-Host "─────────────────────────────────────────" -ForegroundColor Cyan