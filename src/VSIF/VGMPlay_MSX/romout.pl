$fname=@ARGV[0];
open(IN,$fname);

binmode(IN);

$i=0;
while(eof(IN) == 0){
	read(IN,$buf,1);

	if($i<=0x3fff || $i>=0xC000){
	} else {
		print $buf;
	}
	$i++;
}

close(IN);
