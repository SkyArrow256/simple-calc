use std::io;

fn main() {
    for line in io::stdin().lines() {
        let result = calc_rs::calc(&line.unwrap());
        match result {
            Ok(num) => println!("-> {num}"),
            Err(err) => println!("{err:?}"),
        }
    }
}
