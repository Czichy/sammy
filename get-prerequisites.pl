use strict;
use File::Spec;
use File::Path;
use LWP::Simple;
use Archive::Extract;

my $libDir = File::Spec->catdir($0, "..", "..", "lib");
my $cacheDir = File::Spec->catdir($ENV{TEMP}, 'get-req-cache');
mkpath $libDir;

downloadAndExtract("http://sidi-util.googlecode.com/files/Sidi.Util-0.0.zip");

sub download
{
    my ($url, $downloadDir) = @_;
    print "$url\n";
    my @p = split /\//, $url;
    my $filename = File::Spec->catdir($downloadDir, pop @p);
    mkpath $downloadDir;
    mirror($url, $filename);
    return $filename;
}

sub downloadAndExtract
{
    my ($url, $dir, $prefix) = @_;
    $dir or $dir = $libDir;
    $prefix or $prefix = '.';
    my $archive = download($url, $cacheDir);
    my $ae = Archive::Extract->new( archive => $archive );
    $ae->extract( to => File::Spec->catdir($dir, $prefix) );
}
