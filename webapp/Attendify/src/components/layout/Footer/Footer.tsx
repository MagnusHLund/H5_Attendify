import { Image } from '../../ui'
import './Footer.scss'

export function Footer() {
  return (
    <footer className="footer">
      <div className="footer__content">
        <div className="footer__project">
          <span className="footer__name">
            <Image
              src="/internal/logos/Attendify-small.png"
              alt="Attendify Logo"
              className="footer__logo"
            />
            <span className="footer__name--blue">Attend</span>
            <span className="footer__name--green">ify</span>
          </span>
          <span className="footer__description">
            Secure absence monitoring and authentication
          </span>
        </div>
        <div className="footer__copyright">
          © {new Date().getFullYear()} Attendify
        </div>
        <a
          className="footer__github"
          href="https://github.com/MagnusHLund/H5_Attendify"
          target="_blank"
          rel="noopener noreferrer"
          aria-label="View Attendify on GitHub"
        >
          <Image
            src="/external/github/Github.png"
            alt="Github logo"
            className="footer__github-icon"
          />
          <span>View on GitHub</span>
        </a>
      </div>
    </footer>
  )
}
